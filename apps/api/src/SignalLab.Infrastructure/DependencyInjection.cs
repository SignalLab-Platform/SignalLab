using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SignalLab.Infrastructure.Persistence;

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
}