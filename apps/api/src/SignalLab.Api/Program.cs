using Microsoft.Extensions.Diagnostics.HealthChecks;
using SignalLab.Api.Authentication;
using SignalLab.Api.Configuration;
using SignalLab.Api.Users;
using SignalLab.Infrastructure;
using SignalLab.Application;
using SignalLab.Application.Users;
using SignalLab.Application.Files;
using SignalLab.Domain.Files;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.Validate(builder.Environment);

builder.Services.AddSignalLabAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUserContextResolver>();
builder.Services.AddSignalLabCors(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (builder.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (HealthCheckService healthCheckService, CancellationToken cancellationToken) =>
{
    var report = await healthCheckService.CheckHealthAsync(cancellationToken);
    var statusCode = report.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

    return Results.Text(report.Status.ToString(), statusCode: statusCode);
})
.WithName("GetHealth");

app.MapGet(
    "/authentication/session",
    async (
        HttpContext httpContext,
        ResolveCurrentUser resolveCurrentUser,
        CancellationToken cancellationToken) =>
    {
        var externalUserId = httpContext.User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return Results.Unauthorized();
        }

        var user = await resolveCurrentUser.ExecuteAsync(
            externalUserId,
            cancellationToken);

        return Results.Ok(
            new CurrentUserResponse(
                user.Id.Value,
                user.Email));
    })
    .RequireAuthorization()
    .WithName("GetAuthenticationSession");

app.MapGet(
    "/users/me/profile",
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
    .RequireAuthorization()
    .WithName("GetCurrentUserProfile");

app.MapPatch(
    "/users/me/profile/first-name",
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
    .RequireAuthorization()
    .WithName("ChangeCurrentUserFirstName");

app.MapPost(
    "/users/me/profile/email/synchronize",
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
    .RequireAuthorization()
    .WithName("SynchronizeCurrentUserEmail");

app.MapPatch(
    "/users/me/profile/last-name",
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
    .RequireAuthorization()
    .WithName("ChangeCurrentUserLastName");


app.MapPost(
    "/users/me/avatar/upload",
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
    .RequireAuthorization()
    .WithName("CreateCurrentUserAvatarUpload");


    app.MapPost(
    "/users/me/avatar/complete",
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
    .RequireAuthorization()
    .WithName("CompleteCurrentUserAvatarUpload");

app.Run();

public partial class Program
{
}
