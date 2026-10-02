namespace SignalLab.Application.Identity;

public interface IExternalIdentityProvider
{
    Task<ExternalIdentity?> GetByIdAsync(
        string externalIdentityId,
        CancellationToken cancellationToken);
}