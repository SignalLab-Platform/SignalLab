using SignalLab.Application.Storage;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Files;

public sealed class CreateAvatarUpload
{
    private const long MaximumAvatarSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> SupportedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private readonly IStoredFileRepository storedFileRepository;
    private readonly IObjectStorage objectStorage;

    public CreateAvatarUpload(
        IStoredFileRepository storedFileRepository,
        IObjectStorage objectStorage)
    {
        this.storedFileRepository = storedFileRepository;
        this.objectStorage = objectStorage;
    }

    public async Task<CreateAvatarUploadResult> ExecuteAsync(
        UserId ownerUserId,
        string originalFileName,
        string contentType,
        long size,
        CancellationToken cancellationToken)
    {
        var normalizedContentType = contentType?
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedContentType) ||
            !SupportedContentTypes.Contains(normalizedContentType))
        {
            throw new ArgumentException(
                "Content type is not supported for avatars.",
                nameof(contentType));
        }

        if (!SupportedContentTypes.Contains(normalizedContentType))
        {
            throw new ArgumentException(
                "Content type is not supported for avatars.",
                nameof(contentType));
        }

        if (size <= 0 || size > MaximumAvatarSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "Avatar size must be greater than zero and at most 5 MiB.");
        }

        var storedFileId = StoredFileId.New();

        var objectKey =
            $"avatars/{ownerUserId.Value:N}/{storedFileId.Value:N}";

        var storedFile = new StoredFile(
            storedFileId,
            ownerUserId,
            objectKey,
            originalFileName,
            normalizedContentType,
            size,
            DateTimeOffset.UtcNow);

        storedFileRepository.Add(storedFile);

        await storedFileRepository.SaveChangesAsync(
            cancellationToken);

        var uploadUrl = await objectStorage.CreateUploadUrlAsync(
            storedFile.ObjectKey,
            storedFile.ContentType,
            cancellationToken);

        return new CreateAvatarUploadResult(
            storedFile.Id,
            uploadUrl.Url,
            uploadUrl.ExpiresAt);
    }
}