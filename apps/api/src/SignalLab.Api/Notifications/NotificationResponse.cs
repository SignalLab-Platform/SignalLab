using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;

namespace SignalLab.Api.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string TargetType,
    Guid TargetId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt)
{
    public static NotificationResponse From(NotificationDetails details)
    {
        return new NotificationResponse(
            details.Id.Value,
            details.Type.ToString(),
            details.TargetType.ToString(),
            details.TargetId,
            details.CreatedAt,
            details.ReadAt);
    }

    public static NotificationResponse From(Notification notification)
    {
        return From(NotificationDetails.From(notification));
    }
}