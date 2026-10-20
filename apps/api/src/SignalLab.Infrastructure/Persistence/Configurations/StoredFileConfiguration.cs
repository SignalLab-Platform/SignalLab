using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Configurations;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("stored_files");

        builder.HasKey(storedFile => storedFile.Id);

        builder.Property(storedFile => storedFile.Id)
            .HasConversion(
                storedFileId => storedFileId.Value,
                value => new StoredFileId(value))
            .ValueGeneratedNever();

        builder.Property(storedFile => storedFile.OwnerUserId)
            .HasConversion(
                userId => userId.Value,
                value => new UserId(value))
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(storedFile => storedFile.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(storedFile => storedFile.ObjectKey)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(storedFile => storedFile.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(storedFile => storedFile.ContentType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(storedFile => storedFile.Size)
            .IsRequired();

        builder.Property(storedFile => storedFile.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(storedFile => storedFile.CreatedAt)
            .IsRequired();

        builder.HasIndex(storedFile => storedFile.ObjectKey)
            .IsUnique();
    }
}