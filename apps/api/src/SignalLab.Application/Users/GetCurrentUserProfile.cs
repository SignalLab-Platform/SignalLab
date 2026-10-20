using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public sealed class GetCurrentUserProfile
{
    private readonly IUserRepository userRepository;

    public GetCurrentUserProfile(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
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

        return user;
    }
}