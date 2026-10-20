using SignalLab.Application.Files;
using SignalLab.Application.Storage;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Files;

public sealed class GetStoredFileDownloadUrlTests
{
    [Fact]
    public async Task ExecuteAsync_WithoutStoredFileId_ReturnsNull()
    {
        var repository = new FakeStoredFileRepository(null);
        var objectStorage = new FakeObjectStorage();

        var useCase = new GetStoredFileDownloadUrl(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            UserId.New(),
            null,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, repository.GetByIdCallCount);
        Assert.Equal(0, objectStorage.CreateDownloadUrlCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownStoredFile_ReturnsNull()
    {
        var repository = new FakeStoredFileRepository(null);
        var objectStorage = new FakeObjectStorage();

        var useCase = new GetStoredFileDownloadUrl(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            UserId.New(),
            StoredFileId.New(),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(1, repository.GetByIdCallCount);
        Assert.Equal(0, objectStorage.CreateDownloadUrlCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentOwner_ReturnsNull()
    {
        var storedFile = CreateStoredFile(UserId.New());

        var repository = new FakeStoredFileRepository(storedFile);
        var objectStorage = new FakeObjectStorage();

        var useCase = new GetStoredFileDownloadUrl(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            UserId.New(),
            storedFile.Id,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, objectStorage.CreateDownloadUrlCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingStoredFile_ReturnsNull()
    {
        var ownerUserId = UserId.New();
        var storedFile = CreateStoredFile(ownerUserId);

        var repository = new FakeStoredFileRepository(storedFile);
        var objectStorage = new FakeObjectStorage();

        var useCase = new GetStoredFileDownloadUrl(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            ownerUserId,
            storedFile.Id,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, objectStorage.CreateDownloadUrlCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithAvailableStoredFile_ReturnsDownloadUrl()
    {
        var ownerUserId = UserId.New();
        var storedFile = CreateStoredFile(ownerUserId);

        storedFile.MarkAvailable();

        var repository = new FakeStoredFileRepository(storedFile);
        var objectStorage = new FakeObjectStorage();

        var useCase = new GetStoredFileDownloadUrl(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            ownerUserId,
            storedFile.Id,
            CancellationToken.None);

        Assert.Equal(objectStorage.DownloadUrl, result);
        Assert.Equal(1, objectStorage.CreateDownloadUrlCallCount);
        Assert.Equal(
            storedFile.ObjectKey,
            objectStorage.RequestedObjectKey);
    }

    private static StoredFile CreateStoredFile(UserId ownerUserId)
    {
        var storedFileId = StoredFileId.New();

        return new StoredFile(
            storedFileId,
            ownerUserId,
            $"avatars/{ownerUserId.Value:N}/{storedFileId.Value:N}",
            "avatar.png",
            "image/png",
            1024,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeStoredFileRepository : IStoredFileRepository
    {
        private readonly StoredFile? storedFile;

        public FakeStoredFileRepository(StoredFile? storedFile)
        {
            this.storedFile = storedFile;
        }

        public int GetByIdCallCount { get; private set; }

        public Task<StoredFile?> GetByIdAsync(
            StoredFileId storedFileId,
            CancellationToken cancellationToken)
        {
            GetByIdCallCount++;

            if (storedFile?.Id != storedFileId)
            {
                return Task.FromResult<StoredFile?>(null);
            }

            return Task.FromResult<StoredFile?>(storedFile);
        }

        public void Add(StoredFile storedFile)
        {
            throw new NotSupportedException();
        }

        public void Remove(StoredFile storedFile)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        public Uri DownloadUrl { get; } =
            new("https://storage.example.test/download");

        public int CreateDownloadUrlCallCount { get; private set; }

        public string? RequestedObjectKey { get; private set; }

        public Task<UploadUrl> CreateUploadUrlAsync(
            string objectKey,
            string contentType,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Uri> CreateDownloadUrlAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            CreateDownloadUrlCallCount++;
            RequestedObjectKey = objectKey;

            return Task.FromResult(DownloadUrl);
        }

        public Task<ObjectMetadata?> GetMetadataAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}