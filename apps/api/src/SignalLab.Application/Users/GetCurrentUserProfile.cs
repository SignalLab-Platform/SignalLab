using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

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
            throw new ResourceNotFoundException(
                UserErrorCodes.UserNotFound,
                "SignalLab user could not be found.");
        }

        return user;
    }
}