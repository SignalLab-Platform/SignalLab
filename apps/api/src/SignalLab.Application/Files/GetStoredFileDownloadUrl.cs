using SignalLab.Application.Storage;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Files;

public sealed class GetStoredFileDownloadUrl
{
    private readonly IStoredFileRepository storedFileRepository;
    private readonly IObjectStorage objectStorage;

    public GetStoredFileDownloadUrl(
        IStoredFileRepository storedFileRepository,
        IObjectStorage objectStorage)
    {
        this.storedFileRepository = storedFileRepository;
        this.objectStorage = objectStorage;
    }

    public async Task<Uri?> ExecuteAsync(
        UserId ownerUserId,
        StoredFileId? storedFileId,
        CancellationToken cancellationToken)
    {
        if (storedFileId is null)
        {
            return null;
        }

        var storedFile = await storedFileRepository.GetByIdAsync(
            storedFileId.Value,
            cancellationToken);

        if (storedFile is null)
        {
            return null;
        }

        if (storedFile.OwnerUserId != ownerUserId)
        {
            return null;
        }

        if (storedFile.Status != StoredFileStatus.Available)
        {
            return null;
        }

        return await objectStorage.CreateDownloadUrlAsync(
            storedFile.ObjectKey,
            cancellationToken);
    }
}