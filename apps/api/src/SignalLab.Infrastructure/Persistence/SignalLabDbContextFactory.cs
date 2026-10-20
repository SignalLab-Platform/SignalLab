using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SignalLab.Infrastructure.Persistence;

public sealed class SignalLabDbContextFactory
    : IDesignTimeDbContextFactory<SignalLabDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Port=5432;Database=signallab_design_time;Username=design_time;Password=design_time";

    public SignalLabDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<SignalLabDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DesignTimeConnectionString;
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<SignalLabDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new SignalLabDbContext(optionsBuilder.Options);
    }
}