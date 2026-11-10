using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly SignalLabDbContext dbContext;

    public OrganizationRepository(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Organization?> GetByIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        return dbContext.Organizations
            .Include(organization => organization.Roles)
            .Include(organization => organization.Members)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                organization => organization.Id == organizationId,
                cancellationToken);
    }

    public void Add(Organization organization)
    {
        dbContext.Organizations.Add(organization);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Organization>> GetAccessibleByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Organizations
            .AsNoTracking()
            .Where(organization =>
                dbContext.OrganizationMembers.Any(member =>
                    member.OrganizationId == organization.Id &&
                    member.UserId == userId &&
                    member.Status == OrganizationMemberStatus.Active))
            .OrderBy(organization => organization.Name)
            .ThenBy(organization => organization.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Organization>> GetByIdsAsync(
        IReadOnlyCollection<OrganizationId> organizationIds,
        CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return [];
        }

        var ids = organizationIds.Distinct().ToArray();

        return await dbContext.Organizations
            .AsNoTracking()
            .Include(organization => organization.Roles)
            .Where(organization => ids.Contains(organization.Id))
            .ToListAsync(cancellationToken);
    }
}
