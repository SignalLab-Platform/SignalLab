using SignalLab.Application.Errors;

namespace SignalLab.Application.Tests.Errors;

public sealed class ApplicationErrorExceptionTests
{
    [Fact]
    public void ApplicationErrors_ShouldPreserveTheirCodeAndMessage()
    {
        ApplicationErrorException[] exceptions =
        [
            new ValidationException(
                "validation_error",
                "Invalid input."),

            new ResourceNotFoundException(
                "resource_not_found",
                "Resource was not found."),

            new PermissionDeniedException(
                "permission_denied",
                "Permission is required."),

            new ConflictException(
                "conflict",
                "The operation conflicts with the current state."),

            new UnsupportedMediaTypeException(
                "unsupported_media_type",
                "This media type is not supported.")
        ];

        Assert.Equal(5, exceptions.Length);

        foreach (var exception in exceptions)
        {
            Assert.False(string.IsNullOrWhiteSpace(exception.Code));
            Assert.False(string.IsNullOrWhiteSpace(exception.Message));
            Assert.Null(exception.InnerException);
        }

        Assert.Equal("validation_error", exceptions[0].Code);
        Assert.Equal("resource_not_found", exceptions[1].Code);
        Assert.Equal("permission_denied", exceptions[2].Code);
        Assert.Equal("conflict", exceptions[3].Code);
        Assert.Equal("unsupported_media_type", exceptions[4].Code);
    }

    [Fact]
    public void ApplicationError_ShouldPreserveInnerException()
    {
        var originalException = new InvalidOperationException(
            "Original technical error.");

        var exception = new ConflictException(
            "invitation_already_pending",
            "A pending invitation already exists.",
            originalException);

        Assert.Equal(
            "invitation_already_pending",
            exception.Code);

        Assert.Same(
            originalException,
            exception.InnerException);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ApplicationError_ShouldRejectBlankCode(string code)
    {
        Assert.Throws<ArgumentException>(
            () => new ConflictException(
                code,
                "An example conflict."));
    }
}