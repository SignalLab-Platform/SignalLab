using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Id)
            .HasConversion(
                notificationId => notificationId.Value,
                value => new NotificationId(value))
            .ValueGeneratedNever();

        builder.Property(notification => notification.UserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(notification => notification.CreatedAt)
            .IsRequired();

        builder.Property(notification => notification.ReadAt);

        builder.OwnsOne(notification => notification.Target, target =>
        {
            target.Property(value => value.Type)
                .HasColumnName("target_type")
                .HasConversion<string>()
                .HasMaxLength(100)
                .IsRequired();

            target.Property(value => value.Id)
                .HasColumnName("target_id")
                .IsRequired();
        });

        builder.Navigation(notification => notification.Target)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.CreatedAt
        });

        builder.HasIndex(notification => new
        {
            notification.UserId,
            notification.ReadAt
        });
    }
}