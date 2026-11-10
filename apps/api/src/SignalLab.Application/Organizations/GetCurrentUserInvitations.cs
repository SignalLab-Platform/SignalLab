using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed class GetCurrentUserInvitations
{
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly IOrganizationRepository organizationRepository;

    public GetCurrentUserInvitations(
        IOrganizationInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository)
    {
        this.invitationRepository = invitationRepository;
        this.organizationRepository = organizationRepository;
    }

    public async Task<IReadOnlyList<CurrentUserInvitationDetails>> ExecuteAsync(
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        var invitations = await invitationRepository.GetByInvitedUserIdAsync(
            currentUserId,
            cancellationToken);

        if (invitations.Count == 0)
        {
            return [];
        }

        var organizationIds = invitations
            .Select(invitation => invitation.OrganizationId)
            .Distinct()
            .ToArray();

        var organizations = await organizationRepository.GetByIdsAsync(
            organizationIds,
            cancellationToken);

        var organizationsById = organizations.ToDictionary(
            organization => organization.Id);

        var now = DateTimeOffset.UtcNow;

        return invitations
            .Select(invitation =>
            {
                var organization = organizationsById[invitation.OrganizationId];

                var role = organization.Roles.Single(
                    role => role.Id == invitation.RoleId);

                return new CurrentUserInvitationDetails(
                    invitation.Id,
                    invitation.OrganizationId,
                    organization.Name,
                    invitation.InvitedByUserId,
                    invitation.RoleId,
                    role.Name,
                    invitation.GetEffectiveStatus(now),
                    invitation.CreatedAt,
                    invitation.UpdatedAt,
                    invitation.ExpiresAt,
                    invitation.RespondedAt);
            })
            .ToList();
    }
}