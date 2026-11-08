using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Users;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SignalLabDbContext dbContext;

    public UserRepository(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Id == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(
        IReadOnlyCollection<UserId> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var ids = userIds.Distinct().ToArray();

        return await dbContext.Users
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByExternalIdentityIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .SingleOrDefaultAsync(
                user => user.ExternalIdentityId == externalIdentityId,
                cancellationToken);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Email == normalizedEmail,
                cancellationToken);
    }

    public void Add(User user)
    {
        dbContext.Users.Add(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}