using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken);

    void Add(Organization organization);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Organization>> GetAccessibleByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Organization>> GetByIdsAsync(
        IReadOnlyCollection<OrganizationId> organizationIds,
        CancellationToken cancellationToken);
}