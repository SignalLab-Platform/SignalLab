
using SignalLab.Application.Files;
using SignalLab.Application.Users;
using SignalLab.Api.Authentication;

namespace SignalLab.Api.Users;

public static class UserProfileEndpoints
{
    public static IEndpointRouteBuilder MapUserProfileEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/users/me/profile")
            .RequireAuthorization();

        group.MapGet(
            "/",
            async (
                CurrentUserContextResolver currentUserContextResolver,
                GetCurrentUserProfile getCurrentUserProfile,
                GetStoredFileDownloadUrl getStoredFileDownloadUrl,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var user = await getCurrentUserProfile.ExecuteAsync(
                    currentUser.UserId,
                    cancellationToken);

                var avatarUrl = await getStoredFileDownloadUrl.ExecuteAsync(
                    user.Id,
                    user.AvatarFileId,
                    cancellationToken);

                return Results.Ok(
                    new CurrentUserProfileResponse(
                        user.Id.Value,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.Language.ToString(),
                        user.AvatarFileId?.Value,
                        avatarUrl?.ToString()));
            })
            .WithName("GetCurrentUserProfile");

        group.MapPatch(
            "/first-name",
            async (
                ChangeFirstNameRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                ChangeCurrentUserFirstName changeCurrentUserFirstName,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var user = await changeCurrentUserFirstName.ExecuteAsync(
                    currentUser.UserId,
                    request.FirstName,
                    cancellationToken);

                return Results.Ok(
                    new ChangeFirstNameResponse(user.FirstName));
            })
            .WithName("ChangeCurrentUserFirstName");

        group.MapPatch(
            "/last-name",
            async (
                ChangeLastNameRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                ChangeCurrentUserLastName changeCurrentUserLastName,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var user = await changeCurrentUserLastName.ExecuteAsync(
                    currentUser.UserId,
                    request.LastName,
                    cancellationToken);

                return Results.Ok(
                    new ChangeLastNameResponse(user.LastName));
            })
            .WithName("ChangeCurrentUserLastName");

        group.MapPost(
            "/email/synchronize",
            async (
                CurrentUserContextResolver currentUserContextResolver,
                SynchronizeCurrentUserEmail synchronizeCurrentUserEmail,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var user = await synchronizeCurrentUserEmail.ExecuteAsync(
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    new SynchronizeCurrentUserEmailResponse(user.Email));
            })
            .WithName("SynchronizeCurrentUserEmail");

        return endpoints;
    }
}
