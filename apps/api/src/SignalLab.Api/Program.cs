using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.RateLimiting;
using SignalLab.Api.Errors;
using SignalLab.Api.Authentication;
using SignalLab.Api.Configuration;
using SignalLab.Api.Health;
using SignalLab.Api.Users;
using SignalLab.Api.Organizations;
using SignalLab.Api.Notifications;
using SignalLab.Application;
using SignalLab.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.Validate(builder.Environment);

builder.Services.AddSignalLabAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserContextResolver>();
builder.Services.AddSignalLabCors(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOrganizationInvitationRateLimiting(builder.Configuration);

builder.Services.Configure<ExceptionHandlerOptions>(options =>
{
    options.AllowStatusCode404Response = true;
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthEndpoints();

app.MapAuthenticationEndpoints();
app.MapUserProfileEndpoints();
app.MapUserAvatarEndpoints();

app.MapOrganizationEndpoints();
app.MapOrganizationInvitationEndpoints();
app.MapNotificationEndpoints();

app.Run();

public partial class Program
{
}