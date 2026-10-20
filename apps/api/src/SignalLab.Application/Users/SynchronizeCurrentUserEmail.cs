using SignalLab.Application.Identity;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public sealed class SynchronizeCurrentUserEmail
{
    private readonly IUserRepository userRepository;
    private readonly IExternalIdentityProvider externalIdentityProvider;

    public SynchronizeCurrentUserEmail(
        IUserRepository userRepository,
        IExternalIdentityProvider externalIdentityProvider)
    {
        this.userRepository = userRepository;
        this.externalIdentityProvider = externalIdentityProvider;
    }

    public async Task<User> ExecuteAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException(
                $"SignalLab user '{userId.Value}' could not be found.");
        }

        var externalIdentity = await externalIdentityProvider.GetByIdAsync(
            user.ExternalIdentityId,
            cancellationToken);

        if (externalIdentity is null)
        {
            throw new InvalidOperationException(
                $"External identity '{user.ExternalIdentityId}' could not be found.");
        }

        var existingUser = await userRepository.GetByEmailAsync(
            externalIdentity.Email,
            cancellationToken);

        if (existingUser is not null &&
            existingUser.Id != user.Id)
        {
            throw new InvalidOperationException(
                "Email is already associated with another SignalLab user.");
        }

        user.ChangeEmail(externalIdentity.Email);

        await userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }
}