namespace SignalLab.Application.Errors;

public sealed class PermissionDeniedException : ApplicationErrorException
{
    public PermissionDeniedException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}