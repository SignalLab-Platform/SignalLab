
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SignalLab.Api.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/health",
            async (
                HealthCheckService healthCheckService,
                CancellationToken cancellationToken) =>
            {
                var report = await healthCheckService.CheckHealthAsync(
                    cancellationToken);

                var statusCode = report.Status == HealthStatus.Healthy
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status503ServiceUnavailable;

                return Results.Text(
                    report.Status.ToString(),
                    statusCode: statusCode);
            })
            .WithName("GetHealth");

        return endpoints;
    }
}
