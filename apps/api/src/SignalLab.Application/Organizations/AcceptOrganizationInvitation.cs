using SignalLab.Application.Persistence;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class AcceptOrganizationInvitation
{
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly IOrganizationRepository organizationRepository;
    private readonly IUnitOfWork unitOfWork;

    public AcceptOrganizationInvitation(
        IOrganizationInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        this.invitationRepository = invitationRepository;
        this.organizationRepository = organizationRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        OrganizationInvitation? acceptedInvitation = null;

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

                if (invitation.InvitedUserId != currentUserId)
                {
                    throw new ResourceNotFoundException(
                        OrganizationInvitationErrorCodes.InvitationNotFound,
                        "Organization invitation could not be found.");
                }

                var organization = await organizationRepository.GetByIdAsync(
                    invitation.OrganizationId,
                    transactionCancellationToken);

                if (organization is null)
                {
                    throw new ResourceNotFoundException(
                        OrganizationInvitationErrorCodes.OrganizationNotFound,
                        "Organization could not be found.");
                }

                var now = DateTimeOffset.UtcNow;

                OrganizationInvitationTransitionValidator.EnsureModifiable(
                    invitation,
                    now);

                invitation.Accept(currentUserId, now);

                var membership = organization.Members.SingleOrDefault(
                    member => member.UserId == currentUserId);

                if (membership is null)
                {
                    organization.AddMember(
                        currentUserId,
                        invitation.RoleId,
                        now);
                }
                else if (membership.Status == OrganizationMemberStatus.Removed)
                {
                    organization.ReactivateMember(
                        currentUserId,
                        invitation.RoleId,
                        now);
                }
                else
                {
                    throw new ConflictException(
                        OrganizationInvitationErrorCodes.AlreadyMember,
                        "User is already an active member of this organization.");
                }

                await unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                acceptedInvitation = invitation;
            },
            cancellationToken);

        return acceptedInvitation
            ?? throw new InvalidOperationException(
                "Organization invitation was not accepted.");
    }
}