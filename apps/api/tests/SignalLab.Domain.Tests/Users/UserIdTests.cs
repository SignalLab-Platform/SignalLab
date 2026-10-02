using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Users;

public sealed class UserIdTests
{
    [Fact]
    public void New_ShouldCreateNonEmptyId()
    {
        var userId = UserId.New();

        Assert.NotEqual(Guid.Empty, userId.Value);
    }

    [Fact]
    public void SameValue_ShouldBeEqual()
    {
        var value = Guid.NewGuid();

        var first = new UserId(value);
        var second = new UserId(value);

        Assert.Equal(first, second);
    }
}