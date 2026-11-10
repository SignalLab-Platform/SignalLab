using SignalLab.Application.Persistence;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class DeclineOrganizationInvitation
{
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly IUnitOfWork unitOfWork;

    public DeclineOrganizationInvitation(
        IOrganizationInvitationRepository invitationRepository,
        IUnitOfWork unitOfWork)
    {
        this.invitationRepository = invitationRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        OrganizationInvitation? declinedInvitation = null;

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

                var now = UtcNow();

                OrganizationInvitationTransitionValidator.EnsureModifiable(
                    invitation,
                    now);

                invitation.Decline(currentUserId, now);

                await unitOfWork.SaveChangesAsync(
                    transactionCancellationToken);

                declinedInvitation = invitation;
            },
            cancellationToken);

        return declinedInvitation
            ?? throw new InvalidOperationException(
                "Organization invitation was not declined.");
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}