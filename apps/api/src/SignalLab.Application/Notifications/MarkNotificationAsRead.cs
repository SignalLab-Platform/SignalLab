using SignalLab.Application.Persistence;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Notifications;

public sealed class MarkNotificationAsRead
{
    private readonly INotificationRepository notificationRepository;
    private readonly IUnitOfWork unitOfWork;

    public MarkNotificationAsRead(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        this.notificationRepository = notificationRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Notification> ExecuteAsync(
        NotificationId notificationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(
            notificationId,
            cancellationToken);

        if (notification is null)
        {
            throw new ResourceNotFoundException(
                NotificationErrorCodes.NotificationNotFound,
                "Notification could not be found.");
        }

        if (notification.UserId != currentUserId)
        {
            throw new ResourceNotFoundException(
                NotificationErrorCodes.NotificationNotFound,
                "Notification could not be found.");
        }

        notification.MarkAsRead(UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return notification;
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}