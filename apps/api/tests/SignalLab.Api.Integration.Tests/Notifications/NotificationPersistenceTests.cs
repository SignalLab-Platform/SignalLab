using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Notifications;
using SignalLab.Application.Persistence;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Notifications;

[Collection(SignalLabApiCollection.Name)]
public sealed class NotificationPersistenceTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<UserId> createdUserIds = new();

    public NotificationPersistenceTests(SignalLabApiFactory factory)
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
    public async Task CreateNotification_ShouldPersistAndRestoreTarget()
    {
        var user = await CreateTestUserAsync();
        var createdAt = UtcNow();
        var targetId = Guid.NewGuid();

        var notification = CreateNotification(
            user.Id,
            targetId,
            createdAt);

        await PersistNotificationAsync(notification);

        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<INotificationRepository>();

        var restored = await repository.GetByIdAsync(
            notification.Id,
            CancellationToken.None);

        Assert.NotNull(restored);
        Assert.Equal(notification.Id, restored.Id);
        Assert.Equal(user.Id, restored.UserId);
        Assert.Equal(
            NotificationType.OrganizationInvitationReceived,
            restored.Type);
        Assert.NotNull(restored.Target);
        Assert.Equal(
            NotificationTargetType.OrganizationInvitation,
            restored.Target.Type);
        Assert.Equal(targetId, restored.Target.Id);
        Assert.Equal(createdAt, restored.CreatedAt);
        Assert.Null(restored.ReadAt);
    }

    [Fact]
    public async Task MarkAsRead_ShouldPersistReadDate()
    {
        var user = await CreateTestUserAsync();
        var createdAt = UtcNow();

        var notification = CreateNotification(
            user.Id,
            Guid.NewGuid(),
            createdAt);

        await PersistNotificationAsync(notification);

        var readAt = createdAt.AddMinutes(5);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var tracked = await repository.GetByIdAsync(
                notification.Id,
                CancellationToken.None);

            Assert.NotNull(tracked);

            tracked.MarkAsRead(readAt);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var restored = await repository.GetByIdAsync(
                notification.Id,
                CancellationToken.None);

            Assert.NotNull(restored);
            Assert.Equal(readAt, restored.ReadAt);
        }
    }

    [Fact]
    public async Task CountUnread_ShouldExcludeReadAndOtherUserNotifications()
    {
        var firstUser = await CreateTestUserAsync();
        var secondUser = await CreateTestUserAsync();

        var createdAt = UtcNow();

        var firstUnread = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt);

        var secondUnread = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt.AddMinutes(1));

        var readNotification = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt.AddMinutes(2));

        readNotification.MarkAsRead(createdAt.AddMinutes(3));

        var otherUserNotification = CreateNotification(
            secondUser.Id,
            Guid.NewGuid(),
            createdAt);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            repository.Add(firstUnread);
            repository.Add(secondUnread);
            repository.Add(readNotification);
            repository.Add(otherUserNotification);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var firstUserCount = await repository.CountUnreadByUserIdAsync(
                firstUser.Id,
                CancellationToken.None);

            var secondUserCount = await repository.CountUnreadByUserIdAsync(
                secondUser.Id,
                CancellationToken.None);

            Assert.Equal(2, firstUserCount);
            Assert.Equal(1, secondUserCount);
        }
    }

    [Fact]
    public async Task GetRecentNotifications_ShouldRespectUserAndLimit()
    {
        var firstUser = await CreateTestUserAsync();
        var secondUser = await CreateTestUserAsync();

        var createdAt = UtcNow();

        var oldest = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt);

        var middle = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt.AddMinutes(1));

        var newest = CreateNotification(
            firstUser.Id,
            Guid.NewGuid(),
            createdAt.AddMinutes(2));

        var otherUserNotification = CreateNotification(
            secondUser.Id,
            Guid.NewGuid(),
            createdAt.AddMinutes(3));

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            repository.Add(oldest);
            repository.Add(middle);
            repository.Add(newest);
            repository.Add(otherUserNotification);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var results = await repository.GetRecentByUserIdAsync(
                firstUser.Id,
                2,
                CancellationToken.None);

            Assert.Equal(2, results.Count);
            Assert.Equal(newest.Id, results[0].Id);
            Assert.Equal(middle.Id, results[1].Id);
        }
    }

    private async Task<User> CreateTestUserAsync()
    {
        var user = new User(
            $"notification-user-{Guid.NewGuid():N}",
            $"notification-{Guid.NewGuid():N}@example.com",
            UtcNow());

        createdUserIds.Add(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private static Notification CreateNotification(
        UserId userId,
        Guid targetId,
        DateTimeOffset createdAt)
    {
        return new Notification(
            userId,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                targetId),
            createdAt);
    }

    private async Task PersistNotificationAsync(Notification notification)
    {
        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<INotificationRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        repository.Add(notification);

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}