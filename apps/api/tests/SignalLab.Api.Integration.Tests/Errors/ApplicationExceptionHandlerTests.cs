using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Api.Errors;
using SignalLab.Application.Errors;
using SignalLab.Application.Identity;

namespace SignalLab.Api.Integration.Tests.Errors;

public sealed class ApplicationExceptionHandlerTests
{
    public static IEnumerable<object[]> ErrorCases()
    {
        yield return
        [
            new ValidationException(
                "invalid_input",
                "The input is invalid."),
            StatusCodes.Status400BadRequest
        ];

        yield return
        [
            new ResourceNotFoundException(
                "resource_not_found",
                "The resource was not found."),
            StatusCodes.Status404NotFound
        ];

        yield return
        [
            new PermissionDeniedException(
                "permission_denied",
                "Permission is required."),
            StatusCodes.Status403Forbidden
        ];

        yield return
        [
            new ConflictException(
                "invitation_already_pending",
                "A pending invitation already exists."),
            StatusCodes.Status409Conflict
        ];

        yield return
        [
            new UnsupportedMediaTypeException(
                "unsupported_media_type",
                "This media type is not supported."),
            StatusCodes.Status415UnsupportedMediaType
        ];

        yield return
        [
            new ExternalIdentityNotFoundException("external-user-123"),
            StatusCodes.Status401Unauthorized
        ];

        yield return
        [
            new AuthenticationRequiredException(
                AuthenticationErrorCodes.MissingExternalIdentity,
                "The authenticated user has no external identity ID."),
            StatusCodes.Status401Unauthorized
        ];

        yield return
        [
            new ConflictException(
                AuthenticationErrorCodes.SessionInitializationRequired,
                "The SignalLab session must be initialized before accessing this resource."),
            StatusCodes.Status409Conflict
        ];
    }

    [Theory]
    [MemberData(nameof(ErrorCases))]
    public async Task ApplicationError_ShouldProduceProblemDetails(
        ApplicationErrorException exception,
        int expectedStatusCode)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddProblemDetails();

        using var serviceProvider = services.BuildServiceProvider();
        using var responseBody = new MemoryStream();

        var httpContext = new DefaultHttpContext();

        httpContext.RequestServices = serviceProvider;
        httpContext.Request.Headers.Accept = "application/json";
        httpContext.Response.Body = responseBody;

        var handler = new ApplicationExceptionHandler();

        var handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        Assert.True(handled);

        Assert.Equal(
            expectedStatusCode,
            httpContext.Response.StatusCode);

        Assert.StartsWith(
            "application/problem+json",
            httpContext.Response.ContentType);

        responseBody.Position = 0;

        using var document = await JsonDocument.ParseAsync(
            responseBody);

        var root = document.RootElement;

        Assert.Equal(
            expectedStatusCode,
            root.GetProperty("status").GetInt32());

        Assert.Equal(
            exception.Code,
            root.GetProperty("code").GetString());

        Assert.Equal(
            exception.Message,
            root.GetProperty("detail").GetString());

        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("title").GetString()));

        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("type").GetString()));

        Assert.False(root.TryGetProperty(
            "stackTrace",
            out _));
    }

    [Fact]
    public async Task UnexpectedException_ShouldNotBeHandled()
    {
        var exception = new InvalidOperationException(
            "Sensitive internal database details.");

        var httpContext = new DefaultHttpContext();

        var handler = new ApplicationExceptionHandler();

        var handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        Assert.False(handled);

        Assert.False(httpContext.Response.HasStarted);
    }
}