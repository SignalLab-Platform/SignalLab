using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class OrganizationMemberConfiguration
    : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.ToTable("organization_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .HasConversion(
                memberId => memberId.Value,
                value => new OrganizationMemberId(value))
            .ValueGeneratedNever();

        builder.Property(member => member.OrganizationId)
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(member => member.UserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(member => member.RoleId)
            .HasConversion(
                roleId => roleId.Value,
                value => new RoleId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(member => member.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(member => member.JoinedAt)
            .IsRequired();

        builder.Property(member => member.RemovedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(member => new
            {
                member.OrganizationId,
                member.RoleId
            })
            .HasPrincipalKey(role => new
            {
                role.OrganizationId,
                role.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(member => new
        {
            member.OrganizationId,
            member.UserId
        })
        .IsUnique();
    }
}