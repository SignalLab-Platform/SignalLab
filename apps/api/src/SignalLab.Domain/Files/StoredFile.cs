using SignalLab.Domain.Users;

namespace SignalLab.Domain.Files;

public sealed class StoredFile
{
    public StoredFileId Id { get; private set; }

    public UserId OwnerUserId { get; private set; }

    public string ObjectKey { get; private set; }

    public string OriginalFileName { get; private set; }

    public string ContentType { get; private set; }

    public long Size { get; private set; }

    public StoredFileStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private StoredFile()
    {
        ObjectKey = null!;
        OriginalFileName = null!;
        ContentType = null!;
    }

    public StoredFile(
        StoredFileId id,
        UserId ownerUserId,
        string objectKey,
        string originalFileName,
        string contentType,
        long size,
        DateTimeOffset createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Stored file ID cannot be empty.",
                nameof(id));
        }

        if (ownerUserId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Owner user ID cannot be empty.",
                nameof(ownerUserId));
        }

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException(
                "Object key cannot be empty.",
                nameof(objectKey));
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException(
                "Original file name cannot be empty.",
                nameof(originalFileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Content type cannot be empty.",
                nameof(contentType));
        }

        if (size < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "File size cannot be negative.");
        }

        Id = id;
        OwnerUserId = ownerUserId;
        ObjectKey = objectKey.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = contentType.Trim().ToLowerInvariant();
        Size = size;
        Status = StoredFileStatus.Pending;
        CreatedAt = createdAt;
    }

    public void MarkAvailable()
    {
        Status = StoredFileStatus.Available;
    }
}