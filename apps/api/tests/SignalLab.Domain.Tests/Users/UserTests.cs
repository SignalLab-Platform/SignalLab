using SignalLab.Domain.Users;

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
}