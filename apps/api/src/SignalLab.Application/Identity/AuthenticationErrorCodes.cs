namespace SignalLab.Application.Identity;

public static class AuthenticationErrorCodes
{
    public const string MissingExternalIdentity = "missing_external_identity";
    public const string ExternalIdentityNotFound = "external_identity_not_found";
    public const string SessionInitializationRequired = "session_initialization_required";
}