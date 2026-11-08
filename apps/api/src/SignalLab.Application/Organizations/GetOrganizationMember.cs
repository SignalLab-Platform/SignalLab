using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Users;

namespace SignalLab.Application.Organizations;

public sealed class GetOrganizationMembers
{
    private readonly OrganizationAccessService organizationAccessService;
    private readonly IUserRepository userRepository;

    public GetOrganizationMembers(
        OrganizationAccessService organizationAccessService,
        IUserRepository userRepository)
    {
        this.organizationAccessService = organizationAccessService;
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyList<OrganizationMemberDetails>?> ExecuteAsync(
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

        var userIds = organization.Members
            .Select(member => member.UserId)
            .Distinct()
            .ToArray();

        var users = await userRepository.GetByIdsAsync(
            userIds,
            cancellationToken);

        var usersById = users.ToDictionary(user => user.Id);

        var rolesById = organization.Roles.ToDictionary(role => role.Id);

        var members = organization.Members
            .Select(member =>
            {
                var user = usersById[member.UserId];
                var role = rolesById[member.RoleId];

                return new OrganizationMemberDetails(
                    member.Id,
                    member.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    member.RoleId,
                    role.Name,
                    member.Status,
                    member.JoinedAt,
                    member.RemovedAt);
            })
            .OrderBy(member => member.Status)
            .ThenBy(member => member.Email, StringComparer.OrdinalIgnoreCase)
            .ThenBy(member => member.Id.Value)
            .ToList();

        return members;
    }
}