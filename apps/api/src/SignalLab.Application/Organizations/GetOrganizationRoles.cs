using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed class GetOrganizationRoles
{
    private readonly OrganizationAccessService organizationAccessService;

    public GetOrganizationRoles(
        OrganizationAccessService organizationAccessService)
    {
        this.organizationAccessService = organizationAccessService;
    }

    public async Task<IReadOnlyList<Role>?> ExecuteAsync(
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

        return organization.Roles
            .OrderByDescending(role => role.IsOwner)
            .ThenBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(role => role.Id.Value)
            .ToList();
    }
}