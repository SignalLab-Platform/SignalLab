using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Clerk.BackendAPI;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Infrastructure.Persistence.Repositories;
using SignalLab.Infrastructure.Identity;
using SignalLab.Application.Identity;
using SignalLab.Application.Users;

namespace SignalLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetPostgresConnectionString(configuration);

        services.AddDbContext<SignalLabDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        var clerkSecretKey = GetClerkSecretKey(configuration);

        services.AddSingleton<IClerkBackendApi>(
            new ClerkBackendApi(
                bearerAuth: clerkSecretKey));

        services.AddScoped<IExternalIdentityProvider, ClerkExternalIdentityProvider>();

        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }

    private static string GetPostgresConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Required configuration 'ConnectionStrings:Postgres' is missing or empty.");
        }

        try
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(connectionStringBuilder.Host))
            {
                throw new InvalidOperationException(
                    "Configuration 'ConnectionStrings:Postgres' must define a Host.");
            }

            if (string.IsNullOrWhiteSpace(connectionStringBuilder.Database))
            {
                throw new InvalidOperationException(
                    "Configuration 'ConnectionStrings:Postgres' must define a Database.");
            }

            if (string.IsNullOrWhiteSpace(connectionStringBuilder.Username))
            {
                throw new InvalidOperationException(
                    "Configuration 'ConnectionStrings:Postgres' must define a Username.");
            }

            return connectionString;
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException(
                "Configuration 'ConnectionStrings:Postgres' is not a valid PostgreSQL connection string.",
                exception);
        }
    }

    private static string GetClerkSecretKey(IConfiguration configuration)
    {
        var secretKey = configuration["Authentication:Clerk:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "Required configuration 'Authentication:Clerk:SecretKey' is missing or empty.");
        }

        return secretKey;
    }
}