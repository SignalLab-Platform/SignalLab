using SignalLab.Api.Authentication;
using SignalLab.Api.Configuration;
using SignalLab.Api.Health;
using SignalLab.Api.Users;
using SignalLab.Api.Organizations;
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

app.MapHealthEndpoints();
app.MapAuthenticationEndpoints();
app.MapUserProfileEndpoints();
app.MapUserAvatarEndpoints();
app.MapOrganizationEndpoints();

app.Run();

public partial class Program
{
}