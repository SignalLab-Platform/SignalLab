
using SignalLab.Application.Files;
using SignalLab.Domain.Files;
using SignalLab.Api.Authentication;

namespace SignalLab.Api.Users;

public static class UserAvatarEndpoints
{
    public static IEndpointRouteBuilder MapUserAvatarEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/users/me/avatar")
            .RequireAuthorization();

        group.MapPost(
            "/upload",
            async (
                CreateAvatarUploadRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                CreateAvatarUpload createAvatarUpload,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                try
                {
                    var result = await createAvatarUpload.ExecuteAsync(
                        currentUser.UserId,
                        request.FileName,
                        request.ContentType,
                        request.Size,
                        cancellationToken);

                    return Results.Ok(
                        new CreateAvatarUploadResponse(
                            result.StoredFileId.Value,
                            result.UploadUrl.ToString(),
                            result.ExpiresAt));
                }
                catch (ArgumentOutOfRangeException exception)
                    when (exception.ParamName == "size")
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid avatar size.",
                        detail: exception.Message);
                }
                catch (ArgumentException exception)
                    when (exception.ParamName == "contentType")
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status415UnsupportedMediaType,
                        title: "Unsupported avatar content type.",
                        detail: exception.Message);
                }
            })
            .WithName("CreateCurrentUserAvatarUpload");

        group.MapPost(
            "/complete",
            async (
                CompleteAvatarUploadRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                CompleteAvatarUpload completeAvatarUpload,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                await completeAvatarUpload.ExecuteAsync(
                    currentUser.UserId,
                    new StoredFileId(request.FileId),
                    cancellationToken);

                return Results.NoContent();
            })
            .WithName("CompleteCurrentUserAvatarUpload");

        return endpoints;
    }
}
