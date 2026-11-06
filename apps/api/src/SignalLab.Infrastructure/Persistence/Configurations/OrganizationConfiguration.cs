using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Organizations;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration
    : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(organization => organization.Id);

        builder.Property(organization => organization.Id)
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value))
            .ValueGeneratedNever();

        builder.Property(organization => organization.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(organization => organization.Country)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(organization => organization.Website)
            .HasMaxLength(2048);

        builder.Property(organization => organization.CreatedAt)
            .IsRequired();

        builder.Property(organization => organization.UpdatedAt)
            .IsRequired();

        builder.Ignore(organization => organization.OwnerRole);

        builder.HasMany(organization => organization.Roles)
            .WithOne()
            .HasForeignKey(role => role.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(organization => organization.Roles)
            .HasField("roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(organization => organization.Members)
            .WithOne()
            .HasForeignKey(member => member.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(organization => organization.Members)
            .HasField("members")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}