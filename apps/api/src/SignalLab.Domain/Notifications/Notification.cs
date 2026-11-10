using SignalLab.Domain.Users;

namespace SignalLab.Domain.Notifications;

public sealed class Notification
{
    public NotificationId Id { get; private set; }

    public UserId UserId { get; private set; }

    public NotificationType Type { get; private set; }

    public NotificationTarget Target { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    private Notification()
    {
    }

    public Notification(
        UserId userId,
        NotificationType type,
        NotificationTarget target,
        DateTimeOffset createdAt)
    {
        if (userId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId cannot be empty.",
                nameof(userId));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException(
                "Unknown notification type.",
                nameof(type));
        }

        ArgumentNullException.ThrowIfNull(target);

        if (type == NotificationType.OrganizationInvitationReceived
            && target.Type != NotificationTargetType.OrganizationInvitation)
        {
            throw new ArgumentException(
                "Organization invitation notifications must target an invitation.",
                nameof(target));
        }

        Id = NotificationId.New();
        UserId = userId;
        Type = type;
        Target = target;
        CreatedAt = createdAt;
        ReadAt = null;
    }

    public void MarkAsRead(DateTimeOffset now)
    {
        if (ReadAt.HasValue)
        {
            return;
        }

        if (now < CreatedAt)
        {
            throw new InvalidOperationException(
                "Notification cannot be read before its creation.");
        }

        ReadAt = now;
    }
}