using Clerk.BackendAPI;
using SignalLab.Application.Identity;

namespace SignalLab.Infrastructure.Identity;

public sealed class ClerkExternalIdentityProvider : IExternalIdentityProvider
{
    private readonly IClerkBackendApi clerkBackendApi;

    public ClerkExternalIdentityProvider(IClerkBackendApi clerkBackendApi)
    {
        this.clerkBackendApi = clerkBackendApi;
    }

    public async Task<ExternalIdentity?> GetByIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken)
    {
        var userResponse = await clerkBackendApi.Users.GetAsync(
            externalIdentityId);

        var clerkUser = userResponse.User;

        if (clerkUser is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(clerkUser.PrimaryEmailAddressId))
        {
            return null;
        }

        var emailResponse = await clerkBackendApi.EmailAddresses.GetAsync(
            clerkUser.PrimaryEmailAddressId);

        var emailAddress = emailResponse.EmailAddress;

        if (emailAddress is null)
        {
            return null;
        }
        
        var email = emailAddress.EmailAddressValue;

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }
        
        return new ExternalIdentity(
            clerkUser.Id,
            email);
    }
}