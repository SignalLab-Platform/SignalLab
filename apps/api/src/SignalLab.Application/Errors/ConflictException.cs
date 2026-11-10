namespace SignalLab.Application.Errors;

public class ConflictException : ApplicationErrorException
{
    public ConflictException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}