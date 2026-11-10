using SignalLab.Domain.Users;
using SignalLab.Domain.Notifications;

namespace SignalLab.Application.Notifications;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(
        NotificationId notificationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> GetRecentByUserIdAsync(
        UserId userId,
        int limit,
        CancellationToken cancellationToken);

    Task<int> CountUnreadByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken);

    void Add(Notification notification);
}