using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Notifications;

[Collection(SignalLabApiCollection.Name)]
public sealed class GetCurrentUserNotificationsTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;
    private readonly List<UserId> createdUserIds = new();

    public GetCurrentUserNotificationsTests(SignalLabApiFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await factory.Database.ResetUsersAsync(createdUserIds);
    }

    [Fact]
    public async Task NoNotifications_ShouldReturnEmptyList()
    {
        var user = await CreateUserAsync();

        var results = await ExecuteAsync(user.Id);

        Assert.Empty(results);
    }

    [Fact]
    public async Task ShouldReturnOnlyCurrentUserNotifications()
    {
        var currentUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        var currentNotification = await CreateNotificationAsync(
            currentUser.Id,
            UtcNow());

        await CreateNotificationAsync(
            otherUser.Id,
            UtcNow().AddMinutes(1));

        var results = await ExecuteAsync(currentUser.Id);

        var notification = Assert.Single(results);

        Assert.Equal(currentNotification.Id, notification.Id);
    }

    [Fact]
    public async Task ShouldIncludeReadAndUnreadNotifications()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);

        var readNotification = await CreateNotificationAsync(
            user.Id,
            createdAt,
            readAt: createdAt.AddMinutes(10));

        var unreadNotification = await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(20));

        var results = await ExecuteAsync(user.Id);

        Assert.Equal(2, results.Count);

        var read = results.Single(
            item => item.Id == readNotification.Id);

        var unread = results.Single(
            item => item.Id == unreadNotification.Id);

        Assert.Equal(readNotification.ReadAt, read.ReadAt);
        Assert.Null(unread.ReadAt);
    }

    [Fact]
    public async Task ShouldOrderNotificationsByMostRecentFirst()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);

        var oldest = await CreateNotificationAsync(
            user.Id,
            createdAt);

        var newest = await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(20));

        var middle = await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(10));

        var results = await ExecuteAsync(user.Id);

        Assert.Equal(
            new[] { newest.Id, middle.Id, oldest.Id },
            results.Select(item => item.Id));
    }

    [Fact]
    public async Task ShouldLimitRecentNotifications()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddDays(-2);

        var notifications = new List<Notification>();

        for (var index = 0; index < 25; index++)
        {
            notifications.Add(await CreateNotificationAsync(
                user.Id,
                createdAt.AddMinutes(index)));
        }

        var results = await ExecuteAsync(user.Id);

        Assert.Equal(
            GetCurrentUserNotifications.RecentNotificationsLimit,
            results.Count);

        var expectedIds = notifications
            .OrderByDescending(item => item.CreatedAt)
            .Take(GetCurrentUserNotifications.RecentNotificationsLimit)
            .Select(item => item.Id);

        Assert.Equal(
            expectedIds,
            results.Select(item => item.Id));

        Assert.DoesNotContain(
            results,
            item => item.Id == notifications[0].Id);
    }

    [Fact]
    public async Task ShouldPreserveNotificationTargets()
    {
        var user = await CreateUserAsync();
        var targetId = Guid.NewGuid();

        var notification = await CreateNotificationAsync(
            user.Id,
            UtcNow(),
            targetId: targetId);

        var results = await ExecuteAsync(user.Id);

        var details = Assert.Single(results);

        Assert.Equal(notification.Id, details.Id);
        Assert.Equal(notification.Type, details.Type);
        Assert.Equal(notification.CreatedAt, details.CreatedAt);

        Assert.Equal(
            NotificationType.OrganizationInvitationReceived,
            details.Type);

        Assert.Equal(
            NotificationTargetType.OrganizationInvitation,
            details.TargetType);

        Assert.Equal(targetId, details.TargetId);
    }

    [Fact]
    public async Task ReadingNotifications_ShouldNotModifyTheirState()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);
        var readAt = createdAt.AddMinutes(10);

        var readNotification = await CreateNotificationAsync(
            user.Id,
            createdAt,
            readAt: readAt);

        var unreadNotification = await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(20));

        await ExecuteAsync(user.Id);
        await ExecuteAsync(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == user.Id)
            .ToListAsync();

        Assert.Equal(2, stored.Count);

        var storedRead = stored.Single(
            item => item.Id == readNotification.Id);

        var storedUnread = stored.Single(
            item => item.Id == unreadNotification.Id);

        Assert.Equal(readAt, storedRead.ReadAt);
        Assert.Null(storedUnread.ReadAt);

        Assert.Equal(readNotification.CreatedAt, storedRead.CreatedAt);
        Assert.Equal(unreadNotification.CreatedAt, storedUnread.CreatedAt);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = new User(
            $"notification-reader-{Guid.NewGuid():N}",
            $"notification-reader-{Guid.NewGuid():N}@example.com",
            UtcNow());

        createdUserIds.Add(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<Notification> CreateNotificationAsync(
        UserId userId,
        DateTimeOffset createdAt,
        DateTimeOffset? readAt = null,
        Guid? targetId = null)
    {
        var notification = new Notification(
            userId,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                targetId ?? Guid.NewGuid()),
            createdAt);

        if (readAt.HasValue)
        {
            notification.MarkAsRead(readAt.Value);
        }

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Notifications.Add(notification);

        await dbContext.SaveChangesAsync();

        return notification;
    }

    private async Task<IReadOnlyList<NotificationDetails>> ExecuteAsync(
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<GetCurrentUserNotifications>();

        return await useCase.ExecuteAsync(
            currentUserId,
            CancellationToken.None);
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}