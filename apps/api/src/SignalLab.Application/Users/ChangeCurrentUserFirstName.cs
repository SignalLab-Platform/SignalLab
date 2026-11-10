using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

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
            throw new ResourceNotFoundException(
                UserErrorCodes.UserNotFound,
                "SignalLab user could not be found.");
        }

        user.ChangeFirstName(firstName);

        await userRepository.SaveChangesAsync(cancellationToken);

        return user;
    }
}