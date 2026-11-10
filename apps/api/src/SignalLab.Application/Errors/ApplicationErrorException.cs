namespace SignalLab.Application.Errors;

public abstract class ApplicationErrorException : Exception
{
    public string Code { get; }

    protected ApplicationErrorException(
        string code,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "An application error code is required.",
                nameof(code));
        }

        Code = code;
    }
}