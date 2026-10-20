using SignalLab.Application.Files;
using SignalLab.Application.Storage;
using SignalLab.Application.Users;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Files;

public sealed class CompleteAvatarUploadTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidUpload_MarksFileAvailableAndChangesAvatar()
    {
        var user = CreateUser();
        var storedFile = CreateStoredFile(user.Id);

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage = new FakeObjectStorage(
            new ObjectMetadata(
                storedFile.ContentType,
                storedFile.Size));

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await useCase.ExecuteAsync(
            user.Id,
            storedFile.Id,
            CancellationToken.None);

        Assert.Equal(
            StoredFileStatus.Available,
            storedFile.Status);

        Assert.Equal(
            storedFile.Id,
            user.AvatarFileId);

        Assert.Equal(
            storedFile.ObjectKey,
            objectStorage.RequestedObjectKey);

        Assert.Equal(1, objectStorage.GetMetadataCallCount);
        Assert.Equal(1, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingAvatar_ReplacesAndDeletesPreviousAvatar()
    {
        var user = CreateUser();

        var previousAvatar = CreateStoredFile(user.Id);
        previousAvatar.MarkAvailable();
        user.ChangeAvatar(previousAvatar.Id);

        var newAvatar = CreateStoredFile(user.Id);

        var storedFileRepository =
            new FakeStoredFileRepository(
                previousAvatar,
                newAvatar);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage = new FakeObjectStorage(
            new ObjectMetadata(
                newAvatar.ContentType,
                newAvatar.Size));

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await useCase.ExecuteAsync(
            user.Id,
            newAvatar.Id,
            CancellationToken.None);

        Assert.Equal(
            StoredFileStatus.Available,
            newAvatar.Status);

        Assert.Equal(
            newAvatar.Id,
            user.AvatarFileId);

        Assert.Equal(
            previousAvatar,
            storedFileRepository.RemovedStoredFile);

        Assert.Equal(1, objectStorage.DeleteCallCount);

        Assert.Equal(
            previousAvatar.ObjectKey,
            objectStorage.DeletedObjectKey);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownStoredFile_ShouldFail()
    {
        var user = CreateUser();

        var storedFileRepository =
            new FakeStoredFileRepository(null);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage =
            new FakeObjectStorage(null);

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                StoredFileId.New(),
                CancellationToken.None));

        Assert.Null(user.AvatarFileId);
        Assert.Equal(0, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentOwner_ShouldFail()
    {
        var user = CreateUser();

        var storedFile = CreateStoredFile(
            UserId.New());

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage =
            new FakeObjectStorage(null);

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        Assert.Null(user.AvatarFileId);
        Assert.Equal(0, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadyAvailableFile_ShouldFail()
    {
        var user = CreateUser();
        var storedFile = CreateStoredFile(user.Id);

        storedFile.MarkAvailable();

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage =
            new FakeObjectStorage(null);

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.Null(user.AvatarFileId);
        Assert.Equal(0, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingStoredObject_ShouldFailWithoutChanges()
    {
        var user = CreateUser();
        var storedFile = CreateStoredFile(user.Id);

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage =
            new FakeObjectStorage(null);

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        Assert.Null(user.AvatarFileId);
        Assert.Equal(1, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentContentType_ShouldFailWithoutChanges()
    {
        var user = CreateUser();
        var storedFile = CreateStoredFile(user.Id);

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage = new FakeObjectStorage(
            new ObjectMetadata(
                "image/jpeg",
                storedFile.Size));

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        Assert.Null(user.AvatarFileId);
        Assert.Equal(1, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentSize_ShouldFailWithoutChanges()
    {
        var user = CreateUser();
        var storedFile = CreateStoredFile(user.Id);

        var storedFileRepository =
            new FakeStoredFileRepository(storedFile);

        var userRepository =
            new FakeUserRepository(user);

        var objectStorage = new FakeObjectStorage(
            new ObjectMetadata(
                storedFile.ContentType,
                storedFile.Size + 1));

        var useCase = new CompleteAvatarUpload(
            storedFileRepository,
            userRepository,
            objectStorage);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                user.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        Assert.Null(user.AvatarFileId);
        Assert.Equal(1, objectStorage.GetMetadataCallCount);
        Assert.Equal(0, userRepository.SaveChangesCallCount);
    }

    private static User CreateUser()
    {
        return new User(
            $"external-{Guid.NewGuid():N}",
            $"{Guid.NewGuid():N}@example.com",
            DateTimeOffset.UtcNow);
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
        private readonly List<StoredFile> storedFiles;

        public FakeStoredFileRepository(StoredFile? storedFile)
        {
            storedFiles = [];

            if (storedFile is not null)
            {
                storedFiles.Add(storedFile);
            }
        }

        public FakeStoredFileRepository(
            StoredFile firstStoredFile,
            StoredFile secondStoredFile)
        {
            storedFiles =
            [
                firstStoredFile,
                secondStoredFile,
            ];
        }

        public StoredFile? RemovedStoredFile { get; private set; }

        public Task<StoredFile?> GetByIdAsync(
            StoredFileId storedFileId,
            CancellationToken cancellationToken)
        {
            var storedFile = storedFiles.SingleOrDefault(
                storedFile => storedFile.Id == storedFileId);

            return Task.FromResult(storedFile);
        }

        public void Add(StoredFile storedFile)
        {
            throw new NotSupportedException();
        }

        public void Remove(StoredFile storedFile)
        {
            RemovedStoredFile = storedFile;
            storedFiles.Remove(storedFile);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? user;

        public FakeUserRepository(User? user)
        {
            this.user = user;
        }

        public int SaveChangesCallCount { get; private set; }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            if (user?.Id != userId)
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(user);
        }

        public Task<User?> GetByExternalIdentityIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(User user)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        private readonly ObjectMetadata? metadata;

        public FakeObjectStorage(ObjectMetadata? metadata)
        {
            this.metadata = metadata;
        }

        public int GetMetadataCallCount { get; private set; }

        public string? RequestedObjectKey { get; private set; }

        public int DeleteCallCount { get; private set; }

        public string? DeletedObjectKey { get; private set; }

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
            return Task.FromResult(
                new Uri("https://storage.example.test/download"));
        }

        public Task<ObjectMetadata?> GetMetadataAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            GetMetadataCallCount++;
            RequestedObjectKey = objectKey;

            return Task.FromResult(metadata);
        }

        public Task DeleteAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            DeleteCallCount++;
            DeletedObjectKey = objectKey;

            return Task.CompletedTask;
        }
    }
}