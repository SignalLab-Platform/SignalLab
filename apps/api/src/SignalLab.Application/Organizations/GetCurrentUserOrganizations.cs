using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class GetCurrentUserOrganizations
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IUserRepository userRepository;

    public GetCurrentUserOrganizations(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository)
    {
        this.organizationRepository = organizationRepository;
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyList<Organization>> ExecuteAsync(
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

        return await organizationRepository.GetAccessibleByUserIdAsync(
            user.Id,
            cancellationToken);
    }
}
