namespace SignalLab.Application.Errors;

public sealed class ValidationException : ApplicationErrorException
{
    public ValidationException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}