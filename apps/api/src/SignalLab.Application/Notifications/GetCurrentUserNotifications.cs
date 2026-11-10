using SignalLab.Domain.Users;
using SignalLab.Domain.Notifications;

namespace SignalLab.Application.Notifications;

public sealed class GetCurrentUserNotifications
{
    public const int RecentNotificationsLimit = 20;

    private readonly INotificationRepository notificationRepository;

    public GetCurrentUserNotifications(
        INotificationRepository notificationRepository)
    {
        this.notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<NotificationDetails>> ExecuteAsync(
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        var notifications = await notificationRepository.GetRecentByUserIdAsync(
            currentUserId,
            RecentNotificationsLimit,
            cancellationToken);

        return notifications
            .Select(NotificationDetails.From)
            .ToList();
    }
}