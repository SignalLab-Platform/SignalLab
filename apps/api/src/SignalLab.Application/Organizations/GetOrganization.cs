using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed class GetOrganization
{
    private readonly OrganizationAccessService organizationAccessService;

    public GetOrganization(
        OrganizationAccessService organizationAccessService)
    {
        this.organizationAccessService = organizationAccessService;
    }

    public Task<Organization?> ExecuteAsync(
        OrganizationId organizationId,
        UserId userId,
        CancellationToken cancellationToken)
    {
        return organizationAccessService.GetAccessibleAsync(
            organizationId,
            userId,
            cancellationToken);
    }
}