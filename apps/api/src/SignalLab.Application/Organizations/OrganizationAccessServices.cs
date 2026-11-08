using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed class OrganizationAccessService
{
    private readonly IOrganizationRepository organizationRepository;

    public OrganizationAccessService(
        IOrganizationRepository organizationRepository)
    {
        this.organizationRepository = organizationRepository;
    }

    public async Task<Organization?> GetAccessibleAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(
            organizationId,
            cancellationToken);

        if (organization is null)
        {
            return null;
        }

        var hasActiveMembership = organization.Members.Any(member =>
            member.UserId == userId &&
            member.Status == OrganizationMemberStatus.Active);

        return hasActiveMembership
            ? organization
            : null;
    }

    public bool HasPermission(
        Organization organization,
        UserId userId,
        Permission permission)
    {
        var membership = organization.Members.SingleOrDefault(member =>
            member.UserId == userId &&
            member.Status == OrganizationMemberStatus.Active);

        if (membership is null)
        {
            return false;
        }

        var role = organization.Roles.SingleOrDefault(role =>
            role.Id == membership.RoleId);

        return role is not null && role.HasPermission(permission);
    }

    public async Task<OrganizationPermissionResult> GetWithPermissionAsync(
        OrganizationId organizationId,
        UserId userId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        var organization = await GetAccessibleAsync(
            organizationId,
            userId,
            cancellationToken);

        if (organization is null)
        {
            return new OrganizationPermissionResult(
                OrganizationPermissionStatus.OrganizationNotFound,
                null);
        }

        if (!HasPermission(organization, userId, permission))
        {
            return new OrganizationPermissionResult(
                OrganizationPermissionStatus.PermissionDenied,
                null);
        }

        return new OrganizationPermissionResult(
            OrganizationPermissionStatus.Granted,
            organization);
    }
}