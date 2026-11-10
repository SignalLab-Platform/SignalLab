using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Notifications;

[Collection(SignalLabApiCollection.Name)]
public sealed class CountCurrentUserUnreadNotificationsTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;
    private readonly List<UserId> createdUserIds = new();

    public CountCurrentUserUnreadNotificationsTests(SignalLabApiFactory factory)
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
    public async Task NoNotifications_ShouldReturnZero()
    {
        var user = await CreateUserAsync();

        var count = await ExecuteAsync(user.Id);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UnreadNotifications_ShouldBeCounted()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);

        await CreateNotificationAsync(user.Id, createdAt);
        await CreateNotificationAsync(user.Id, createdAt.AddMinutes(1));
        await CreateNotificationAsync(user.Id, createdAt.AddMinutes(2));

        var count = await ExecuteAsync(user.Id);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task ReadNotifications_ShouldBeExcluded()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);

        await CreateNotificationAsync(
            user.Id,
            createdAt,
            readAt: createdAt.AddMinutes(5));

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(1),
            readAt: createdAt.AddMinutes(6));

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(2));

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(3));

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(4));

        var count = await ExecuteAsync(user.Id);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task OtherUsersNotifications_ShouldBeExcluded()
    {
        var currentUser = await CreateUserAsync();
        var otherUser = await CreateUserAsync();
        var createdAt = UtcNow();

        await CreateNotificationAsync(currentUser.Id, createdAt);

        await CreateNotificationAsync(otherUser.Id, createdAt);
        await CreateNotificationAsync(otherUser.Id, createdAt);

        var currentCount = await ExecuteAsync(currentUser.Id);
        var otherCount = await ExecuteAsync(otherUser.Id);

        Assert.Equal(1, currentCount);
        Assert.Equal(2, otherCount);
    }

    [Fact]
    public async Task Count_ShouldNotBeLimitedToRecentTwenty()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddDays(-2);

        for (var index = 0; index < 25; index++)
        {
            await CreateNotificationAsync(
                user.Id,
                createdAt.AddMinutes(index));
        }

        var count = await ExecuteAsync(user.Id);

        Assert.Equal(25, count);

        using var scope = factory.Services.CreateScope();

        var recentUseCase = scope.ServiceProvider
            .GetRequiredService<GetCurrentUserNotifications>();

        var recent = await recentUseCase.ExecuteAsync(
            user.Id,
            CancellationToken.None);

        Assert.Equal(20, recent.Count);
    }

    [Fact]
    public async Task CountingNotifications_ShouldNotModifyReadState()
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
            createdAt.AddMinutes(1));

        Assert.Equal(1, await ExecuteAsync(user.Id));
        Assert.Equal(1, await ExecuteAsync(user.Id));

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == user.Id)
            .ToListAsync();

        Assert.Equal(2, notifications.Count);

        var storedRead = notifications.Single(
            item => item.Id == readNotification.Id);

        var storedUnread = notifications.Single(
            item => item.Id == unreadNotification.Id);

        Assert.Equal(readAt, storedRead.ReadAt);
        Assert.Null(storedUnread.ReadAt);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = new User(
            $"notification-counter-{Guid.NewGuid():N}",
            $"notification-counter-{Guid.NewGuid():N}@example.com",
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
        DateTimeOffset? readAt = null)
    {
        var notification = new Notification(
            userId,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                Guid.NewGuid()),
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

    private async Task<int> ExecuteAsync(UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<CountCurrentUserUnreadNotifications>();

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