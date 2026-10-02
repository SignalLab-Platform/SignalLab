using Microsoft.Extensions.Diagnostics.HealthChecks;
using SignalLab.Api.Authentication;
using SignalLab.Infrastructure;
using SignalLab.Api.Configuration;
using SignalLab.Application;
using SignalLab.Application.Users;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.Validate(builder.Environment);

builder.Services.AddSignalLabAuthentication(builder.Configuration);
builder.Services.AddSignalLabCors(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
{
    var report = await healthCheckService.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

    return Results.Text(report.Status.ToString(), statusCode: statusCode);
})
.WithName("GetHealth");

app.MapGet(
    "/authentication/session",
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

        return Results.Ok(new
        {
            UserId = user.Id.Value,
            user.Email,
        });
    })
    .RequireAuthorization()
    .WithName("GetAuthenticationSession");

app.Run();

public partial class Program
{
}
