using SignalLab.Domain.Notifications;

namespace SignalLab.Application.Notifications;

public sealed record NotificationDetails(
    NotificationId Id,
    NotificationType Type,
    NotificationTargetType TargetType,
    Guid TargetId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt)
{
    public static NotificationDetails From(Notification notification)
    {
        return new NotificationDetails(
            notification.Id,
            notification.Type,
            notification.Target.Type,
            notification.Target.Id,
            notification.CreatedAt,
            notification.ReadAt);
    }
}