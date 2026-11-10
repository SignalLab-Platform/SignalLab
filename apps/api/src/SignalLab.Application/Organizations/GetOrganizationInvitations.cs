using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed class GetOrganizationInvitations
{
    private readonly OrganizationAccessService organizationAccessService;
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly IUserRepository userRepository;

    public GetOrganizationInvitations(
        OrganizationAccessService organizationAccessService,
        IOrganizationInvitationRepository invitationRepository,
        IUserRepository userRepository)
    {
        this.organizationAccessService = organizationAccessService;
        this.invitationRepository = invitationRepository;
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyList<OrganizationInvitationDetails>?> ExecuteAsync(
        OrganizationId organizationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        var organization = await organizationAccessService.GetAccessibleAsync(
            organizationId,
            currentUserId,
            cancellationToken);

        if (organization is null)
        {
            return null;
        }

        var invitations = await invitationRepository.GetByOrganizationIdAsync(
            organizationId,
            cancellationToken);

        var userIds = invitations
            .Select(invitation => invitation.InvitedUserId)
            .Distinct()
            .ToArray();

        var users = await userRepository.GetByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = users.ToDictionary(user => user.Id);

        var now = DateTimeOffset.UtcNow;

        return invitations
            .Select(invitation =>
            {
                var invitedUser = usersById[invitation.InvitedUserId];

                return OrganizationInvitationDetails.From(
                    invitation,
                    organization,
                    invitedUser,
                    now);
            })
            .ToList();
    }
}