namespace SignalLab.Application.Errors;

public sealed class AuthenticationRequiredException : ApplicationErrorException
{
    public AuthenticationRequiredException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}