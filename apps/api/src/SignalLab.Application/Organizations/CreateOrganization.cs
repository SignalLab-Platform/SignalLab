using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Users;

namespace SignalLab.Application.Organizations;

public sealed class CreateOrganization
{
    private readonly IOrganizationRepository organizationRepository;
    private readonly IUserRepository userRepository;

    public CreateOrganization(
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository)
    {
        this.organizationRepository = organizationRepository;
        this.userRepository = userRepository;
    }

    public async Task<Organization> ExecuteAsync(
        UserId creatorId,
        string name,
        string country,
        string? website,
        CancellationToken cancellationToken)
    {
        var creator = await userRepository.GetByIdAsync(
            creatorId,
            cancellationToken);

        if (creator is null)
        {
            throw new InvalidOperationException(
                $"SignalLab user '{creatorId.Value}' could not be found.");
        }

        var organization = new Organization(
            creator.Id,
            name,
            country,
            website,
            DateTimeOffset.UtcNow);

        organizationRepository.Add(organization);

        await organizationRepository.SaveChangesAsync(cancellationToken);

        return organization;
    }
}