using SignalLab.Application.Storage;

namespace SignalLab.Api.Integration.Tests.Infrastructure;

public sealed class TestObjectStorage : IObjectStorage
{
    private readonly Func<ObjectMetadata?> getMetadata;
    private readonly Action<string> onGetMetadata;

    public TestObjectStorage(
        Func<ObjectMetadata?> getMetadata,
        Action<string> onGetMetadata)
    {
        this.getMetadata = getMetadata;
        this.onGetMetadata = onGetMetadata;
    }

    public Task<UploadUrl> CreateUploadUrlAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            new UploadUrl(
                new Uri("https://storage.example.test/upload"),
                DateTimeOffset.UtcNow.AddMinutes(10)));
    }

    public Task<Uri> CreateDownloadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(
            new Uri("https://storage.example.test/download"));
    }

    public Task<ObjectMetadata?> GetMetadataAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        onGetMetadata(objectKey);

        return Task.FromResult(getMetadata());
    }

    public Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
