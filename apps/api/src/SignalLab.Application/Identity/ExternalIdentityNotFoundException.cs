using SignalLab.Application.Errors;

namespace SignalLab.Application.Identity;

public sealed class ExternalIdentityNotFoundException : ApplicationErrorException
{
    public ExternalIdentityNotFoundException(string externalIdentityId)
        : base(
            AuthenticationErrorCodes.ExternalIdentityNotFound,
            "External identity could not be resolved.")
    {
    }
}