using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Notifications;

public sealed class NotificationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_ShouldCreateUnreadNotification()
    {
        var userId = UserId.New();
        var target = CreateTarget();

        var notification = new Notification(
            userId,
            NotificationType.OrganizationInvitationReceived,
            target,
            CreatedAt);

        Assert.NotEqual(Guid.Empty, notification.Id.Value);
        Assert.Equal(userId, notification.UserId);
        Assert.Equal(
            NotificationType.OrganizationInvitationReceived,
            notification.Type);
        Assert.Equal(target, notification.Target);
        Assert.Equal(CreatedAt, notification.CreatedAt);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyUserId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Notification(
                new UserId(Guid.Empty),
                NotificationType.OrganizationInvitationReceived,
                CreateTarget(),
                CreatedAt));
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownType()
    {
        Assert.Throws<ArgumentException>(() =>
            new Notification(
                UserId.New(),
                (NotificationType)999,
                CreateTarget(),
                CreatedAt));
    }

    [Fact]
    public void Constructor_ShouldRejectNullTarget()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new Notification(
                UserId.New(),
                NotificationType.OrganizationInvitationReceived,
                null!,
                CreatedAt));
    }

    [Fact]
    public void MarkAsRead_ShouldSetReadAt()
    {
        var notification = CreateNotification();
        var now = CreatedAt.AddHours(1);

        notification.MarkAsRead(now);

        Assert.Equal(now, notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_ShouldBeIdempotent()
    {
        var notification = CreateNotification();
        var firstReadAt = CreatedAt.AddHours(1);

        notification.MarkAsRead(firstReadAt);
        notification.MarkAsRead(CreatedAt.AddHours(2));

        Assert.Equal(firstReadAt, notification.ReadAt);
    }

    [Fact]
    public void MarkAsRead_ShouldRejectDateBeforeCreation()
    {
        var notification = CreateNotification();

        Assert.Throws<InvalidOperationException>(() =>
            notification.MarkAsRead(CreatedAt.AddTicks(-1)));

        Assert.Null(notification.ReadAt);
    }

    private static Notification CreateNotification()
    {
        return new Notification(
            UserId.New(),
            NotificationType.OrganizationInvitationReceived,
            CreateTarget(),
            CreatedAt);
    }

    private static NotificationTarget CreateTarget()
    {
        return new NotificationTarget(
            NotificationTargetType.OrganizationInvitation,
            Guid.NewGuid());
    }
}