namespace SignalLab.Application.Identity;

public sealed class ExternalIdentityNotFoundException : Exception
{
    public ExternalIdentityNotFoundException(string externalIdentityId)
        : base($"External identity '{externalIdentityId}' could not be found.")
    {
    }
}