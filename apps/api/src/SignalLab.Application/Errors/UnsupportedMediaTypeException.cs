namespace SignalLab.Application.Errors;

public sealed class UnsupportedMediaTypeException : ApplicationErrorException
{
    public UnsupportedMediaTypeException(
        string code,
        string message,
        Exception? innerException = null)
        : base(code, message, innerException)
    {
    }
}