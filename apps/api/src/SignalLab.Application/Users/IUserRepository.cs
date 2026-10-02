using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public interface IUserRepository
{
    Task<User?> GetByExternalIdentityIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}