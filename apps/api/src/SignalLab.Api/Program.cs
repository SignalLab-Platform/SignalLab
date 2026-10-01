using Microsoft.Extensions.Diagnostics.HealthChecks;
using SignalLab.Api.Authentication;
using SignalLab.Infrastructure;
using SignalLab.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.Validate(builder.Environment);

builder.Services.AddSignalLabAuthentication(builder.Configuration);
builder.Services.AddSignalLabCors(builder.Configuration);
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

app.MapGet("/authentication/session", (HttpContext httpContext) =>
{
    var externalUserId = httpContext.User.FindFirst("sub")?.Value;

    return Results.Ok(new
    {
        ExternalUserId = externalUserId,
    });
})
.RequireAuthorization()
.WithName("GetAuthenticationSession");

app.Run();

public partial class Program
{
}
