using SignalLab.Domain.Users;

namespace SignalLab.Application.Users;

public sealed class ChangeCurrentUserLastName
{
    private readonly IUserRepository userRepository;

    public ChangeCurrentUserLastName(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<User> ExecuteAsync(
        UserId userId,
        string? lastName,
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

        user.ChangeLastName(lastName);

        await userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }
}