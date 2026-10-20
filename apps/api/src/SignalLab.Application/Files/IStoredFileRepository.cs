using SignalLab.Domain.Files;

namespace SignalLab.Application.Files;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(
        StoredFileId storedFileId,
        CancellationToken cancellationToken);

    void Add(StoredFile storedFile);

    void Remove(StoredFile storedFile);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}