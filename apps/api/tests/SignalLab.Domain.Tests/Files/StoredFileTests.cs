using SignalLab.Domain.Files;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Files;

public sealed class StoredFileTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreatePendingStoredFile()
    {
        var storedFileId = StoredFileId.New();
        var ownerUserId = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var storedFile = new StoredFile(
            storedFileId,
            ownerUserId,
            "avatars/file-123",
            "avatar.PNG",
            "IMAGE/PNG",
            1024,
            createdAt);

        Assert.Equal(storedFileId, storedFile.Id);
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new StoredFile(
                new StoredFileId(Guid.Empty),
                UserId.New(),
                "avatars/file-123",
                "avatar.png",
                "image/png",
                1024,
                DateTimeOffset.UtcNow));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyOwnerUserId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new StoredFile(
                StoredFileId.New(),
                new UserId(Guid.Empty),
                "avatars/file-123",
                "avatar.png",
                "image/png",
                1024,
                DateTimeOffset.UtcNow));

        Assert.Equal("ownerUserId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyObjectKey_ShouldFail(
        string objectKey)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new StoredFile(
                StoredFileId.New(),
                UserId.New(),
                objectKey,
                "avatar.png",
                "image/png",
                1024,
                DateTimeOffset.UtcNow));

        Assert.Equal("objectKey", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyOriginalFileName_ShouldFail(
        string originalFileName)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new StoredFile(
                StoredFileId.New(),
                UserId.New(),
                "avatars/file-123",
                originalFileName,
                "image/png",
                1024,
                DateTimeOffset.UtcNow));

        Assert.Equal("originalFileName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyContentType_ShouldFail(
        string contentType)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new StoredFile(
                StoredFileId.New(),
                UserId.New(),
                "avatars/file-123",
                "avatar.png",
                contentType,
                1024,
                DateTimeOffset.UtcNow));

        Assert.Equal("contentType", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeSize_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StoredFile(
                StoredFileId.New(),
                UserId.New(),
                "avatars/file-123",
                "avatar.png",
                "image/png",
                -1,
                DateTimeOffset.UtcNow));

        Assert.Equal("size", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldNormalizeStoredValues()
    {
        var storedFile = new StoredFile(
            StoredFileId.New(),
            UserId.New(),
            "  avatars/file-123  ",
            "  avatar.png  ",
            "  IMAGE/PNG  ",
            1024,
            DateTimeOffset.UtcNow);

        Assert.Equal("avatars/file-123", storedFile.ObjectKey);
        Assert.Equal("avatar.png", storedFile.OriginalFileName);
        Assert.Equal("image/png", storedFile.ContentType);
    }

    [Fact]
    public void MarkAvailable_ShouldMakeStoredFileAvailable()
    {
        var storedFile = new StoredFile(
            StoredFileId.New(),
            UserId.New(),
            "avatars/file-123",
            "avatar.png",
            "image/png",
            1024,
            DateTimeOffset.UtcNow);

        storedFile.MarkAvailable();

        Assert.Equal(
            StoredFileStatus.Available,
            storedFile.Status);
    }
}