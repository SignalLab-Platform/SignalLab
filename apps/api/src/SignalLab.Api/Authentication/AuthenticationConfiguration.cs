using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SignalLab.Api.Authentication;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddSignalLabAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Authentication:Clerk:Issuer"];

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "Required configuration 'Authentication:Clerk:Issuer' is missing or empty.");
        }

        var authorizedParties = configuration
            .GetSection("Authentication:Clerk:AuthorizedParties")
            .Get<string[]>() ?? [];

        if (authorizedParties.Length == 0 ||
            authorizedParties.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "Required configuration 'Authentication:Clerk:AuthorizedParties' is missing or empty.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = issuer;
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var authorizedParty = context.Principal?.FindFirst("azp")?.Value;

                        if (string.IsNullOrWhiteSpace(authorizedParty) ||
                            !authorizedParties.Contains(
                                authorizedParty,
                                StringComparer.Ordinal))
                        {
                            context.Fail("Token authorized party is not allowed.");
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();

        return services;
    }
} 