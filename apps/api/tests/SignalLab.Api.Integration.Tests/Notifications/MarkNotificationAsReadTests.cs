using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Errors;

namespace SignalLab.Api.Integration.Tests.Notifications;

[Collection(SignalLabApiCollection.Name)]
public sealed class MarkNotificationAsReadTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;
    private readonly List<UserId> createdUserIds = new();

    public MarkNotificationAsReadTests(SignalLabApiFactory factory)
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
    public async Task UnreadNotification_ShouldBeMarkedAsRead()
    {
        var user = await CreateUserAsync();

        var notification = await CreateNotificationAsync(
            user.Id,
            UtcNow().AddHours(-1));

        var result = await ExecuteAsync(notification.Id, user.Id);

        Assert.NotNull(result.ReadAt);
        Assert.True(result.ReadAt >= notification.CreatedAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.Notifications
            .AsNoTracking()
            .SingleAsync(item => item.Id == notification.Id);

        Assert.Equal(result.ReadAt, stored.ReadAt);
        Assert.Equal(notification.CreatedAt, stored.CreatedAt);
    }

    [Fact]
    public async Task AlreadyReadNotification_ShouldPreserveOriginalReadAt()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);
        var originalReadAt = createdAt.AddMinutes(10);

        var notification = await CreateNotificationAsync(
            user.Id,
            createdAt,
            originalReadAt);

        var result = await ExecuteAsync(notification.Id, user.Id);

        Assert.Equal(originalReadAt, result.ReadAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.Notifications
            .AsNoTracking()
            .SingleAsync(item => item.Id == notification.Id);

        Assert.Equal(originalReadAt, stored.ReadAt);
    }

    [Fact]
    public async Task OtherUsersNotification_ShouldBeRejected()
    {
        var owner = await CreateUserAsync();
        var otherUser = await CreateUserAsync();

        var notification = await CreateNotificationAsync(
            owner.Id,
            UtcNow().AddHours(-1));

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
           () => ExecuteAsync(notification.Id, otherUser.Id));

        Assert.Equal(
            NotificationErrorCodes.NotificationNotFound,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.Notifications
            .AsNoTracking()
            .SingleAsync(item => item.Id == notification.Id);

        Assert.Equal(owner.Id, stored.UserId);
        Assert.Null(stored.ReadAt);
    }

    [Fact]
    public async Task UnknownNotification_ShouldBeRejected()
    {
        var user = await CreateUserAsync();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(
                new NotificationId(Guid.NewGuid()),
                user.Id));

        Assert.Equal(
            NotificationErrorCodes.NotificationNotFound,
            exception.Code);
    }

    [Fact]
    public async Task MarkingNotificationAsRead_ShouldDecreaseUnreadCount()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);

        var first = await CreateNotificationAsync(
            user.Id,
            createdAt);

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(1));

        await CreateNotificationAsync(
            user.Id,
            createdAt.AddMinutes(2));

        Assert.Equal(3, await CountUnreadAsync(user.Id));

        await ExecuteAsync(first.Id, user.Id);

        Assert.Equal(2, await CountUnreadAsync(user.Id));
    }

    [Fact]
    public async Task MarkingNotificationAsRead_ShouldPreserveHistoryAndTarget()
    {
        var user = await CreateUserAsync();
        var createdAt = UtcNow().AddHours(-1);
        var targetId = Guid.NewGuid();

        var notification = await CreateNotificationAsync(
            user.Id,
            createdAt,
            targetId: targetId);

        await ExecuteAsync(notification.Id, user.Id);

        using var scope = factory.Services.CreateScope();

        var getNotifications = scope.ServiceProvider
            .GetRequiredService<GetCurrentUserNotifications>();

        var results = await getNotifications.ExecuteAsync(
            user.Id,
            CancellationToken.None);

        var details = Assert.Single(results);

        Assert.Equal(notification.Id, details.Id);
        Assert.Equal(notification.Type, details.Type);
        Assert.Equal(notification.CreatedAt, details.CreatedAt);

        Assert.Equal(
            NotificationTargetType.OrganizationInvitation,
            details.TargetType);

        Assert.Equal(targetId, details.TargetId);
        Assert.NotNull(details.ReadAt);
    }

    [Fact]
    public async Task RepeatedMarkAsRead_ShouldBeIdempotent()
    {
        var user = await CreateUserAsync();

        var notification = await CreateNotificationAsync(
            user.Id,
            UtcNow().AddHours(-1));

        var first = await ExecuteAsync(notification.Id, user.Id);

        Assert.NotNull(first.ReadAt);

        var second = await ExecuteAsync(notification.Id, user.Id);

        Assert.Equal(first.ReadAt, second.ReadAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.Notifications
            .AsNoTracking()
            .SingleAsync(item => item.Id == notification.Id);

        Assert.Equal(first.ReadAt, stored.ReadAt);
        Assert.Equal(0, await CountUnreadAsync(user.Id));
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

    private async Task<Notification> ExecuteAsync(
        NotificationId notificationId,
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<MarkNotificationAsRead>();

        return await useCase.ExecuteAsync(
            notificationId,
            currentUserId,
            CancellationToken.None);
    }

    private async Task<int> CountUnreadAsync(UserId currentUserId)
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