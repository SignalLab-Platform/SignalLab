namespace SignalLab.Application.Storage;

public interface IObjectStorage
{
    Task<UploadUrl> CreateUploadUrlAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken);

    Task<Uri> CreateDownloadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken);

    Task<ObjectMetadata?> GetMetadataAsync(
        string objectKey,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken);
}