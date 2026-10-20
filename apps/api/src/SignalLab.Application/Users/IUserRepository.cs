using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task<User?> GetByExternalIdentityIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}