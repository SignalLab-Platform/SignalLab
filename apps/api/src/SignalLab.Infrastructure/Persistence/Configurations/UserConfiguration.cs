using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Users;
using SignalLab.Domain.Files;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(user => user.ExternalIdentityId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.FirstName)
            .HasMaxLength(100);

        builder.Property(user => user.LastName)
            .HasMaxLength(100);

        builder.Property(user => user.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(user => user.AvatarFileId)
            .HasConversion(
                storedFileId => storedFileId.HasValue
                    ? storedFileId.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? new StoredFileId(value.Value)
                    : null);

        builder.HasOne<StoredFile>()
            .WithMany()
            .HasForeignKey(user => user.AvatarFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasIndex(user => user.ExternalIdentityId)
            .IsUnique();

        builder.HasIndex(user => user.Email)
            .IsUnique();
    }
}