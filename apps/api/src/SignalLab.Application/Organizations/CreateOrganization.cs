using SignalLab.Application.Errors;
using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

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
            throw new ResourceNotFoundException(
                UserErrorCodes.UserNotFound,
                "SignalLab user could not be found.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                OrganizationErrorCodes.InvalidName,
                "Organization name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(country) ||
            !IsoCountryCodes.Contains(country.Trim()))
        {
            throw new ValidationException(
                OrganizationErrorCodes.InvalidCountry,
                "Country must be a valid ISO 3166-1 alpha-2 code.");
        }

        if (!string.IsNullOrWhiteSpace(website))
        {
            var normalizedWebsite = website.Trim();

            if (!Uri.TryCreate(
                normalizedWebsite,
                UriKind.Absolute,
                out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ValidationException(
                    OrganizationErrorCodes.InvalidWebsite,
                    "Website must be a valid absolute HTTP or HTTPS URL.");
            }
        }

        var organization = new Organization(
            creator.Id,
            name,
            country,
            website,
            DateTimeOffset.UtcNow);

        organizationRepository.Add(organization);

        await organizationRepository.SaveChangesAsync(
            cancellationToken);

        return organization;
    }
}