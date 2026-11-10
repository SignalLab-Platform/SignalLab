using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly SignalLabDbContext dbContext;

    public NotificationRepository(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Notification?> GetByIdAsync(
        NotificationId notificationId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications
            .SingleOrDefaultAsync(
                notification => notification.Id == notificationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetRecentByUserIdAsync(
        UserId userId,
        int limit,
        CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        return await dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ThenBy(notification => notification.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountUnreadByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return dbContext.Notifications
            .CountAsync(
                notification =>
                    notification.UserId == userId &&
                    notification.ReadAt == null,
                cancellationToken);
    }

    public void Add(Notification notification)
    {
        dbContext.Notifications.Add(notification);
    }
}