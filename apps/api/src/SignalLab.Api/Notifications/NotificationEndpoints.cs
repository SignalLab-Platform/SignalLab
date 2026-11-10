using SignalLab.Api.Authentication;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;

namespace SignalLab.Api.Notifications;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/users/me/notifications")
            .RequireAuthorization();

        group.MapGet(
            "/",
            async (
                CurrentUserContextResolver currentUserContextResolver,
                GetCurrentUserNotifications getCurrentUserNotifications,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var notifications = await getCurrentUserNotifications.ExecuteAsync(
                    currentUser.UserId,
                    cancellationToken);

                var response = notifications
                    .Select(NotificationResponse.From)
                    .ToList();

                return Results.Ok(response);
            })
            .WithName("GetCurrentUserNotifications");

        group.MapGet(
            "/unread-count",
            async (
                CurrentUserContextResolver currentUserContextResolver,
                CountCurrentUserUnreadNotifications countCurrentUserUnreadNotifications,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var count = await countCurrentUserUnreadNotifications.ExecuteAsync(
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    new UnreadNotificationCountResponse(count));
            })
            .WithName("CountCurrentUserUnreadNotifications");

        group.MapPost(
            "/{notificationId:guid}/read",
            async (
                Guid notificationId,
                CurrentUserContextResolver currentUserContextResolver,
                MarkNotificationAsRead markNotificationAsRead,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var notification = await markNotificationAsRead.ExecuteAsync(
                    new NotificationId(notificationId),
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    NotificationResponse.From(notification));
            })
            .WithName("MarkNotificationAsRead");

        return endpoints;
    }
}