
using SignalLab.Application.Users;
using SignalLab.Api.Users;

namespace SignalLab.Api.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/authentication")
            .RequireAuthorization();

        group.MapGet(
            "/session",
            async (
                HttpContext httpContext,
                ResolveCurrentUser resolveCurrentUser,
                CancellationToken cancellationToken) =>
            {
                var externalUserId = httpContext.User.FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(externalUserId))
                {
                    return Results.Unauthorized();
                }

                var user = await resolveCurrentUser.ExecuteAsync(
                    externalUserId,
                    cancellationToken);

                return Results.Ok(
                    new CurrentUserResponse(
                        user.Id.Value,
                        user.Email));
            })
            .WithName("GetAuthenticationSession");

        return endpoints;
    }
}
