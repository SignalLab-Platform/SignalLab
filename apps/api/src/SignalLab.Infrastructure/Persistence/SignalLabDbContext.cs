using Microsoft.EntityFrameworkCore;
using SignalLab.Domain.Users;
using SignalLab.Domain.Files;

namespace SignalLab.Infrastructure.Persistence;

public sealed class SignalLabDbContext : DbContext
{
    public SignalLabDbContext(DbContextOptions<SignalLabDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SignalLabDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}