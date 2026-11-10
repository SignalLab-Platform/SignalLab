namespace SignalLab.Domain.Notifications;

public sealed record NotificationTarget
{
    public NotificationTargetType Type { get; }

    public Guid Id { get; }

    public NotificationTarget(NotificationTargetType type, Guid id)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException(
                "Unknown notification target type.",
                nameof(type));
        }

        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Notification target Id cannot be empty.",
                nameof(id));
        }

        Type = type;
        Id = id;
    }
}