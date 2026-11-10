using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using SignalLab.Application.Errors;
using SignalLab.Application.Identity;

namespace SignalLab.Api.Errors;

public sealed class ApplicationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badRequestException &&
            badRequestException.StatusCode == StatusCodes.Status400BadRequest)
        {
            await Results.Problem(
                title: ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
                detail: "The request body is malformed or does not match the expected format.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = "invalid_request_body"
                })
                .ExecuteAsync(httpContext);

            return true;
        }

        if (exception is not ApplicationErrorException applicationException)
        {
            return false;
        }

        var statusCode = applicationException switch
        {
            AuthenticationRequiredException => StatusCodes.Status401Unauthorized,
            ExternalIdentityNotFoundException => StatusCodes.Status401Unauthorized,
            ValidationException => StatusCodes.Status400BadRequest,
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            PermissionDeniedException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            UnsupportedMediaTypeException => StatusCodes.Status415UnsupportedMediaType,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return false;
        }

        await Results.Problem(
            statusCode: statusCode,
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            detail: applicationException.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = applicationException.Code
            })
            .ExecuteAsync(httpContext);

        return true;
    }
}