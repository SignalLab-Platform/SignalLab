using SignalLab.Domain.Users;

namespace SignalLab.Application.Notifications;

public sealed class CountCurrentUserUnreadNotifications
{
    private readonly INotificationRepository notificationRepository;

    public CountCurrentUserUnreadNotifications(
        INotificationRepository notificationRepository)
    {
        this.notificationRepository = notificationRepository;
    }

    public Task<int> ExecuteAsync(
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        return notificationRepository.CountUnreadByUserIdAsync(
            currentUserId,
            cancellationToken);
    }
}