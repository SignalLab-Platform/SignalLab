using SignalLab.Application.Identity;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public sealed class ResolveCurrentUser
{
    private readonly IUserRepository userRepository;
    private readonly IExternalIdentityProvider externalIdentityProvider;

    public ResolveCurrentUser(
        IUserRepository userRepository,
        IExternalIdentityProvider externalIdentityProvider)
    {
        this.userRepository = userRepository;
        this.externalIdentityProvider = externalIdentityProvider;
    }

    public async Task<User> ExecuteAsync(
        string externalIdentityId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdentityIdAsync(externalIdentityId, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        var externalIdentity = await externalIdentityProvider.GetByIdAsync(externalIdentityId, cancellationToken);

        if (externalIdentity is null)
        {
            throw new ExternalIdentityNotFoundException(externalIdentityId);
        }

        var newUser = new User(externalIdentity.Id, externalIdentity.Email, DateTimeOffset.UtcNow);

        userRepository.Add(newUser);

        await userRepository.SaveChangesAsync(cancellationToken);

        return newUser;
    }
}