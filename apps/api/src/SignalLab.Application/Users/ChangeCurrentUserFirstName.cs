using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public sealed class ChangeCurrentUserFirstName
{
    private readonly IUserRepository userRepository;

    public ChangeCurrentUserFirstName(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<User> ExecuteAsync(
        UserId userId,
        string? firstName,
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

        user.ChangeFirstName(firstName);

        await userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }
}