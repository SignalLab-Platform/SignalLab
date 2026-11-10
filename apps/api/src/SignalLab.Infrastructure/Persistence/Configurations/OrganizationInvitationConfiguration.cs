using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class OrganizationInvitationConfiguration
    : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.ToTable("organization_invitations");

        builder.HasKey(invitation => invitation.Id);

        builder.Property(invitation => invitation.Id)
            .HasConversion(
                invitationId => invitationId.Value,
                value => new OrganizationInvitationId(value))
            .ValueGeneratedNever();

        builder.Property(invitation => invitation.OrganizationId)
            .HasConversion(
                organizationId => organizationId.Value,
                value => new OrganizationId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(invitation => invitation.InvitedUserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(invitation => invitation.InvitedByUserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(invitation => invitation.RoleId)
            .HasConversion(
                roleId => roleId.Value,
                value => new RoleId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(invitation => invitation.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(invitation => invitation.CreatedAt)
            .IsRequired();

        builder.Property(invitation => invitation.UpdatedAt)
            .IsRequired();

        builder.Property(invitation => invitation.ExpiresAt)
            .IsRequired();

        builder.Property(invitation => invitation.RespondedAt);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(invitation => invitation.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(invitation => invitation.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(invitation => new
            {
                invitation.OrganizationId,
                invitation.RoleId
            })
            .HasPrincipalKey(role => new
            {
                role.OrganizationId,
                role.Id
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(invitation => new
        {
            invitation.OrganizationId,
            invitation.InvitedUserId
        })
        .IsUnique()
        .HasDatabaseName("ux_organization_invitations_pending")
        .HasFilter("\"Status\" = 'Pending'");

        builder.HasIndex(invitation => new
        {
            invitation.InvitedUserId,
            invitation.CreatedAt
        });

        builder.HasIndex(invitation => new
        {
            invitation.OrganizationId,
            invitation.CreatedAt
        });
    }
}