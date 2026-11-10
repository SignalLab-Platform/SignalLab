using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SignalLab.Api.Organizations;

public static class OrganizationInvitationRateLimiting
{
    public const string UserPolicy = "organization-invitation-create-user";

    public static IServiceCollection AddOrganizationInvitationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var userLimit = configuration.GetValue(
            "OrganizationInvitations:RateLimiting:UserPermitLimit",
            10);

        var organizationLimit = configuration.GetValue(
            "OrganizationInvitations:RateLimiting:OrganizationPermitLimit",
            40);

        if (userLimit <= 0 || organizationLimit <= 0)
        {
            throw new InvalidOperationException(
                "Organization invitation rate limits must be positive.");
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddPolicy(UserPolicy, context =>
            {
                var userId = context.User.FindFirst("sub")?.Value
                    ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? "unidentified";

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"user:{userId}",
                    _ => CreateWindowOptions(userLimit));
            });

            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var endpointName = context.GetEndpoint()?
                        .Metadata.GetMetadata<IEndpointNameMetadata>()?
                        .EndpointName;

                    if (endpointName != "CreateOrganizationInvitation")
                    {
                        return RateLimitPartition.GetNoLimiter("other-endpoints");
                    }

                    var organizationId =
                        context.Request.RouteValues["organizationId"]?
                            .ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"organization:{organizationId}",
                        _ => CreateWindowOptions(organizationLimit));
                });

            options.OnRejected = (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<
                        ILogger<OrganizationInvitationRateLimitLog>>();

                logger.LogWarning(
                    "Organization invitation rate limit exceeded. OrganizationId={OrganizationId}",
                    context.HttpContext.Request.RouteValues["organizationId"]);

                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                return ValueTask.CompletedTask;
            };
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateWindowOptions(int limit)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        };
    }

    private sealed class OrganizationInvitationRateLimitLog;
}