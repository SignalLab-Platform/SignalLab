using SignalLab.Application.Files;
using SignalLab.Application.Storage;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Files;

public sealed class CreateAvatarUploadTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesPendingStoredFileAndUploadUrl()
    {
        var ownerUserId = UserId.New();
        var repository = new FakeStoredFileRepository();
        var objectStorage = new FakeObjectStorage();

        var useCase = new CreateAvatarUpload(
            repository,
            objectStorage);

        var result = await useCase.ExecuteAsync(
            ownerUserId,
            "avatar.png",
            "image/png",
            1024,
            CancellationToken.None);

        var storedFile = Assert.IsType<StoredFile>(
            repository.AddedStoredFile);

        Assert.Equal(ownerUserId, storedFile.OwnerUserId);
        Assert.Equal("avatar.png", storedFile.OriginalFileName);
        Assert.Equal("image/png", storedFile.ContentType);
        Assert.Equal(1024, storedFile.Size);
        Assert.Equal(StoredFileStatus.Pending, storedFile.Status);

        Assert.Equal(
            $"avatars/{ownerUserId.Value:N}/{storedFile.Id.Value:N}",
            storedFile.ObjectKey);

        Assert.Equal(storedFile.Id, result.StoredFileId);
        Assert.Equal(objectStorage.UploadUrl.Url, result.UploadUrl);
        Assert.Equal(
            objectStorage.UploadUrl.ExpiresAt,
            result.ExpiresAt);

        Assert.Equal(storedFile.ObjectKey, objectStorage.ObjectKey);
        Assert.Equal("image/png", objectStorage.ContentType);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public async Task ExecuteAsync_WithSupportedContentType_ShouldSucceed(
        string contentType)
    {
        var repository = new FakeStoredFileRepository();
        var objectStorage = new FakeObjectStorage();

        var useCase = new CreateAvatarUpload(
            repository,
            objectStorage);

        await useCase.ExecuteAsync(
            UserId.New(),
            "avatar",
            contentType,
            1024,
            CancellationToken.None);

        Assert.NotNull(repository.AddedStoredFile);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    public async Task ExecuteAsync_WithUnsupportedContentType_ShouldFail(
        string contentType)
    {
        var repository = new FakeStoredFileRepository();
        var objectStorage = new FakeObjectStorage();

        var useCase = new CreateAvatarUpload(
            repository,
            objectStorage);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                UserId.New(),
                "avatar",
                contentType,
                1024,
                CancellationToken.None));

        Assert.Null(repository.AddedStoredFile);
        Assert.Equal(0, repository.SaveChangesCallCount);
        Assert.Equal(0, objectStorage.CreateUploadUrlCallCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5 * 1024 * 1024 + 1)]
    public async Task ExecuteAsync_WithInvalidSize_ShouldFail(long size)
    {
        var repository = new FakeStoredFileRepository();
        var objectStorage = new FakeObjectStorage();

        var useCase = new CreateAvatarUpload(
            repository,
            objectStorage);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecuteAsync(
                UserId.New(),
                "avatar.png",
                "image/png",
                size,
                CancellationToken.None));

        Assert.Null(repository.AddedStoredFile);
        Assert.Equal(0, repository.SaveChangesCallCount);
        Assert.Equal(0, objectStorage.CreateUploadUrlCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithMaximumSize_ShouldSucceed()
    {
        var repository = new FakeStoredFileRepository();
        var objectStorage = new FakeObjectStorage();

        var useCase = new CreateAvatarUpload(
            repository,
            objectStorage);

        await useCase.ExecuteAsync(
            UserId.New(),
            "avatar.png",
            "image/png",
            5 * 1024 * 1024,
            CancellationToken.None);

        Assert.NotNull(repository.AddedStoredFile);
    }

    private sealed class FakeStoredFileRepository : IStoredFileRepository
    {
        public StoredFile? AddedStoredFile { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<StoredFile?> GetByIdAsync(
            StoredFileId storedFileId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<StoredFile?>(null);
        }

        public void Add(StoredFile storedFile)
        {
            AddedStoredFile = storedFile;
        }

        public void Remove(StoredFile storedFile)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        public UploadUrl UploadUrl { get; } = new(
            new Uri("https://upload.example.test/avatar"),
            DateTimeOffset.UtcNow.AddMinutes(10));

        public int CreateUploadUrlCallCount { get; private set; }

        public string? ObjectKey { get; private set; }

        public string? ContentType { get; private set; }

        public Task<UploadUrl> CreateUploadUrlAsync(
            string objectKey,
            string contentType,
            CancellationToken cancellationToken)
        {
            CreateUploadUrlCallCount++;
            ObjectKey = objectKey;
            ContentType = contentType;

            return Task.FromResult(UploadUrl);
        }

        public Task<Uri> CreateDownloadUrlAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new Uri("https://download.example.test/avatar"));
        }

        public Task<ObjectMetadata?> GetMetadataAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ObjectMetadata?>(null);
        }

        public Task DeleteAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}