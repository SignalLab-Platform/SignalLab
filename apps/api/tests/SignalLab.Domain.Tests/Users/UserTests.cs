using SignalLab.Domain.Users;
using SignalLab.Domain.Files;

namespace SignalLab.Domain.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateUser()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var user = new User(
            "user_external_123",
            "user@example.test",
            createdAt);

        Assert.NotEqual(Guid.Empty, user.Id.Value);
        Assert.Equal("user_external_123", user.ExternalIdentityId);
        Assert.Equal("user@example.test", user.Email);
        Assert.Null(user.FirstName);
        Assert.Null(user.AvatarFileId);
        Assert.Equal(createdAt, user.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyExternalIdentityId_ShouldFail(
        string externalIdentityId)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new User(
                externalIdentityId,
                "user@example.test",
                DateTimeOffset.UtcNow));

        Assert.Equal("externalIdentityId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyEmail_ShouldFail(string email)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new User(
                "user_external_123",
                email,
                DateTimeOffset.UtcNow));

        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldNormalizeEmail()
    {
        var user = new User(
            "user_external_123",
            " User@Example.TEST ",
            DateTimeOffset.UtcNow);

        Assert.Equal("user@example.test", user.Email);
    }

    [Fact]
    public void ChangeEmail_WithValidValue_ShouldNormalizeAndChangeEmail()
    {
        var user = new User(
            "user_external_123",
            "old@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeEmail(" New@Example.TEST ");

        Assert.Equal("new@example.test", user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeEmail_WithEmptyValue_ShouldFail(string email)
    {
        var user = new User(
            "user_external_123",
            "old@example.test",
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(() =>
            user.ChangeEmail(email));

        Assert.Equal("email", exception.ParamName);
        Assert.Equal("old@example.test", user.Email);
    }

    [Fact]
    public void ChangeFirstName_WithValue_ShouldChangeFirstName()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeFirstName("Jimmy");

        Assert.Equal("Jimmy", user.FirstName);
    }

    [Fact]
    public void ChangeFirstName_ShouldNormalizeFirstName()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeFirstName("  Jimmy  ");

        Assert.Equal("Jimmy", user.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeFirstName_WithEmptyValue_ShouldClearFirstName(
        string? firstName)
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeFirstName("Jimmy");

        user.ChangeFirstName(firstName);

        Assert.Null(user.FirstName);
    }

    [Fact]
    public void ChangeLastName_ShouldNormalizeValue()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeLastName("  Fromonot  ");

        Assert.Equal("Fromonot", user.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ChangeLastName_WithEmptyValue_ShouldClearValue(
        string? lastName)
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeLastName("Fromonot");

        user.ChangeLastName(lastName);

        Assert.Null(user.LastName);
    }

    [Fact]
    public void Constructor_ShouldUseEnglishAsDefaultLanguage()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        Assert.Equal(UserLanguage.English, user.Language);
    }

    [Fact]
    public void ChangeLanguage_ShouldChangeLanguage()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeLanguage(UserLanguage.French);

        Assert.Equal(UserLanguage.French, user.Language);
    }

    [Fact]
    public void ChangeAvatar_ShouldSetAvatarFileId()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        var avatarFileId = StoredFileId.New();

        user.ChangeAvatar(avatarFileId);

        Assert.Equal(avatarFileId, user.AvatarFileId);
    }

    [Fact]
    public void RemoveAvatar_ShouldClearAvatarFileId()
    {
        var user = new User(
            "user_external_123",
            "user@example.test",
            DateTimeOffset.UtcNow);

        user.ChangeAvatar(StoredFileId.New());

        user.RemoveAvatar();

        Assert.Null(user.AvatarFileId);
    }
}
