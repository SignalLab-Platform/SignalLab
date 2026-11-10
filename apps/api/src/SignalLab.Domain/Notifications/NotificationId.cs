namespace SignalLab.Domain.Notifications;

public readonly record struct NotificationId(Guid Value)
{
    public static NotificationId New()
    {
        return new NotificationId(Guid.NewGuid());
    }
}