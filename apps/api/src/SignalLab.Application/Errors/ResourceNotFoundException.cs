namespace SignalLab.Application.Errors;

public sealed class ResourceNotFoundException : ApplicationErrorException
{
    public ResourceNotFoundException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}