using SignalLab.Application.Identity;

namespace SignalLab.Api.Integration.Tests.Infrastructure;

public sealed class TestExternalIdentityProvider : IExternalIdentityProvider
{
    private readonly Func<ExternalIdentity> getIdentity;
    private readonly Action onGetById;

    public TestExternalIdentityProvider(
        Func<ExternalIdentity> getIdentity,
        Action onGetById)
    {
        this.getIdentity = getIdentity;
        this.onGetById = onGetById;
    }

    public Task<ExternalIdentity?> GetByIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken)
    {
        onGetById();

        var identity = getIdentity();

        if (identity.Id != externalIdentityId)
        {
            return Task.FromResult<ExternalIdentity?>(null);
        }

        return Task.FromResult<ExternalIdentity?>(identity);
    }
}
