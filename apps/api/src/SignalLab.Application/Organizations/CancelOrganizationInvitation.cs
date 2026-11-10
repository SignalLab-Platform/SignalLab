using SignalLab.Application.Persistence;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class CancelOrganizationInvitation
{
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly OrganizationAccessService organizationAccessService;
    private readonly IUnitOfWork unitOfWork;

    public CancelOrganizationInvitation(
        IOrganizationInvitationRepository invitationRepository,
        OrganizationAccessService organizationAccessService,
        IUnitOfWork unitOfWork)
    {
        this.invitationRepository = invitationRepository;
        this.organizationAccessService = organizationAccessService;
        this.unitOfWork = unitOfWork;
    }

    public async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        OrganizationInvitation? cancelledInvitation = null;

        await unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var invitation = await invitationRepository.GetByIdForUpdateAsync(
                    invitationId,
                    transactionCancellationToken);

                if (invitation is null)
                {
                    throw new ResourceNotFoundException(
                        OrganizationInvitationErrorCodes.InvitationNotFound,
                        "Organization invitation could not be found.");
                }

                var access = await organizationAccessService.GetWithPermissionAsync(
                    invitation.OrganizationId,
                    currentUserId,
                    Permission.ManageMembers,
                    transactionCancellationToken);

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

                var now = UtcNow();

                OrganizationInvitationTransitionValidator.EnsureModifiable(
                    invitation,
                    now);

                invitation.Cancel(now);

                await unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                cancelledInvitation = invitation;
            },
            cancellationToken);

        return cancelledInvitation
            ?? throw new InvalidOperationException(
                "Organization invitation was not cancelled.");
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}