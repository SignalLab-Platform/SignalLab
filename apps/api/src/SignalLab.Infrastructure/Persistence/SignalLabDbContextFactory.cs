using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SignalLab.Infrastructure.Persistence;

public sealed class SignalLabDbContextFactory
    : IDesignTimeDbContextFactory<SignalLabDbContext>
{
    public SignalLabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SignalLabDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=signallab;Username=signallab;Password=design-time");

        return new SignalLabDbContext(optionsBuilder.Options);
    }
}