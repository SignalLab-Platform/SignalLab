using Microsoft.EntityFrameworkCore;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence;

public sealed class SignalLabDbContext : DbContext
{
    public SignalLabDbContext(DbContextOptions<SignalLabDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SignalLabDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}