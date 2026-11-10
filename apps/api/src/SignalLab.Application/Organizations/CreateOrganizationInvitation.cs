using EmailAddressAttribute = System.ComponentModel.DataAnnotations.EmailAddressAttribute;
using SignalLab.Application.Notifications;
using SignalLab.Application.Persistence;
using SignalLab.Application.Users;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class CreateOrganizationInvitation
{
    private readonly OrganizationAccessService organizationAccessService;
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly INotificationRepository notificationRepository;
    private readonly IUserRepository userRepository;
    private readonly IUnitOfWork unitOfWork;

    public CreateOrganizationInvitation(
        OrganizationAccessService organizationAccessService,
        IOrganizationInvitationRepository invitationRepository,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        this.organizationAccessService = organizationAccessService;
        this.invitationRepository = invitationRepository;
        this.notificationRepository = notificationRepository;
        this.userRepository = userRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationId organizationId,
        UserId invitedByUserId,
        UserId invitedUserId,
        RoleId roleId,
        OrganizationInvitationDuration duration,
        CancellationToken cancellationToken)
    {
        var access = await organizationAccessService.GetWithPermissionAsync(
            organizationId,
            invitedByUserId,
            Permission.ManageMembers,
            cancellationToken);

        if (access.Status == OrganizationPermissionStatus.OrganizationNotFound)
        {
            throw new ResourceNotFoundException(
                OrganizationInvitationErrorCodes.OrganizationNotFound,
                "Organization could not be found.");
        }

        if (access.Status == OrganizationPermissionStatus.PermissionDenied)
        {
            throw new PermissionDeniedException(
                OrganizationInvitationErrorCodes.ManageMembersDenied,
                "User cannot manage members of this organization.");
        }

        if (access.Organization is null)
        {
            throw new InvalidOperationException(
                "Organization access was granted without an organization.");
        }

        var organization = access.Organization;

        var invitedUser = await userRepository.GetByIdAsync(
            invitedUserId,
            cancellationToken);

        if (invitedUser is null)
        {
            throw new ResourceNotFoundException(
                OrganizationInvitationErrorCodes.RecipientUnavailable,
                "The invitation could not be created for this recipient.");
        }

        var role = organization.Roles.SingleOrDefault(candidate => candidate.Id == roleId);

        if (role is null)
        {
            throw new ValidationException(
                OrganizationInvitationErrorCodes.InvalidRole,
                "Role does not belong to this organization.");
        }

        if (role.Id == organization.OwnerRole.Id)
        {
            var inviterIsOwner = organization.Members.Any(member =>
                member.UserId == invitedByUserId &&
                member.Status == OrganizationMemberStatus.Active &&
                member.RoleId == organization.OwnerRole.Id);

            if (!inviterIsOwner)
            {
                throw new PermissionDeniedException(
                    OrganizationInvitationErrorCodes.OwnerInvitationDenied,
                    "Only an active organization owner can invite another owner.");
            }
        }

        var activeMembership = organization.Members.Any(member =>
            member.UserId == invitedUserId &&
            member.Status == OrganizationMemberStatus.Active);

        if (activeMembership)
        {
            throw new ConflictException(
                OrganizationInvitationErrorCodes.AlreadyMember,
                "User is already an active member of this organization.");
        }

        var now = DateTimeOffset.UtcNow;

        var expiration = duration switch
        {
            OrganizationInvitationDuration.OneDay => now.AddDays(1),
            OrganizationInvitationDuration.SevenDays => now.AddDays(7),
            OrganizationInvitationDuration.ThirtyDays => now.AddDays(30),
            _ => throw new ValidationException(
                OrganizationInvitationErrorCodes.InvalidDuration,
                "Organization invitation duration is invalid.")
        };

        OrganizationInvitation? createdInvitation = null;

        await unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var existingInvitation =
                    await invitationRepository.GetPendingAsync(
                        organizationId,
                        invitedUserId,
                        transactionCancellationToken);

                if (existingInvitation is not null)
                {
                    if (existingInvitation.GetEffectiveStatus(now) ==
                        OrganizationInvitationStatus.Pending)
                    {
                        throw new OrganizationInvitationAlreadyPendingException();
                    }

                    existingInvitation.Expire(now);

                    await unitOfWork.SaveChangesAsync(
                        transactionCancellationToken);
                }

                var invitation = new OrganizationInvitation(
                    organization.Id,
                    invitedUser.Id,
                    invitedByUserId,
                    role.Id,
                    now,
                    expiration);

                var notification = new Notification(
                    invitedUser.Id,
                    NotificationType.OrganizationInvitationReceived,
                    new NotificationTarget(
                        NotificationTargetType.OrganizationInvitation,
                        invitation.Id.Value),
                    now);

                invitationRepository.Add(invitation);
                notificationRepository.Add(notification);

                await unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                createdInvitation = invitation;
            },
            cancellationToken);

        return createdInvitation
            ?? throw new InvalidOperationException(
                "Invitation was not created.");
    }

    public async Task<OrganizationInvitation> ExecuteByEmailAsync(
        OrganizationId organizationId,
        UserId invitedByUserId,
        string? email,
        RoleId roleId,
        OrganizationInvitationDuration duration,
        CancellationToken cancellationToken)
    {
        var access = await organizationAccessService.GetWithPermissionAsync(
            organizationId,
            invitedByUserId,
            Permission.ManageMembers,
            cancellationToken);

        if (access.Status == OrganizationPermissionStatus.OrganizationNotFound)
        {
            throw new ResourceNotFoundException(
                OrganizationInvitationErrorCodes.OrganizationNotFound,
                "Organization could not be found.");
        }

        if (access.Status == OrganizationPermissionStatus.PermissionDenied)
        {
            throw new PermissionDeniedException(
                OrganizationInvitationErrorCodes.ManageMembersDenied,
                "User cannot manage members of this organization.");
        }

        if (string.IsNullOrWhiteSpace(email) ||
            !new EmailAddressAttribute().IsValid(email.Trim()))
        {
            throw new ValidationException(
                OrganizationInvitationErrorCodes.InvalidRecipientEmail,
                "Invited user email must be a valid email address.");
        }

        var invitedUser = await userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (invitedUser is null)
        {
            throw new ResourceNotFoundException(
                OrganizationInvitationErrorCodes.RecipientUnavailable,
                "The invitation could not be created for this recipient.");
        }

        return await ExecuteAsync(
            organizationId,
            invitedByUserId,
            invitedUser.Id,
            roleId,
            duration,
            cancellationToken);
    }
}