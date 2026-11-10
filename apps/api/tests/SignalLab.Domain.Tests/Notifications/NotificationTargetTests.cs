using SignalLab.Domain.Notifications;

namespace SignalLab.Domain.Tests.Notifications;

public sealed class NotificationTargetTests
{
    [Fact]
    public void Constructor_ShouldStoreTarget()
    {
        var id = Guid.NewGuid();

        var target = new NotificationTarget(
            NotificationTargetType.OrganizationInvitation,
            id);

        Assert.Equal(NotificationTargetType.OrganizationInvitation, target.Type);
        Assert.Equal(id, target.Id);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyId()
    {
        Assert.Throws<ArgumentException>(() =>
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                Guid.Empty));
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownType()
    {
        Assert.Throws<ArgumentException>(() =>
            new NotificationTarget(
                (NotificationTargetType)999,
                Guid.NewGuid()));
    }

    [Fact]
    public void TargetsWithSameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();

        var first = new NotificationTarget(
            NotificationTargetType.OrganizationInvitation,
            id);

        var second = new NotificationTarget(
            NotificationTargetType.OrganizationInvitation,
            id);

        Assert.Equal(first, second);
    }
}