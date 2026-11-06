using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Organizations;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("organization_roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Id)
            .HasConversion(
                roleId => roleId.Value,
                value => new RoleId(value))
            .ValueGeneratedNever();

        builder.Property(role => role.OrganizationId)
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.Property(role => role.IsOwner)
            .IsRequired();

        builder.Property(role => role.CreatedAt)
            .IsRequired();

        builder.Property(role => role.UpdatedAt)
            .IsRequired();

        builder.Ignore(role => role.Permissions);

        builder.Property<List<Permission>>("permissions")
            .HasField("permissions")
            .HasColumnName("permissions")
            .HasColumnType("jsonb")
            .HasConversion(
                value => SerializePermissions(value),
                value => DeserializePermissions(value))
            .Metadata.SetValueComparer(
                new ValueComparer<List<Permission>>(
                    (left, right) =>
                        left != null &&
                        right != null &&
                        left.SequenceEqual(right),
                    value => GetPermissionsHashCode(value),
                    value => value.ToList()));

        builder.HasAlternateKey(
            role => new { role.OrganizationId, role.Id });

        builder.HasIndex(role => role.OrganizationId)
            .IsUnique()
            .HasFilter("\"IsOwner\" = TRUE");
    }

    private static string SerializePermissions(List<Permission> permissions)
    {
        return JsonSerializer.Serialize(permissions);
    }

    private static List<Permission> DeserializePermissions(string json)
    {
        return JsonSerializer.Deserialize<List<Permission>>(json)
            ?? new List<Permission>();
    }

    private static int GetPermissionsHashCode(List<Permission> permissions)
    {
        var hash = new HashCode();

        foreach (var permission in permissions)
        {
            hash.Add(permission);
        }

        return hash.ToHashCode();
    }
}