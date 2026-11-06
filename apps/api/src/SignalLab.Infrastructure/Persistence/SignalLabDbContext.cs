using Microsoft.EntityFrameworkCore;
using SignalLab.Domain.Users;
using SignalLab.Domain.Files;
using SignalLab.Domain.Organizations;

namespace SignalLab.Infrastructure.Persistence;

public sealed class SignalLabDbContext : DbContext
{
    public SignalLabDbContext(
        DbContextOptions<SignalLabDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SignalLabDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}