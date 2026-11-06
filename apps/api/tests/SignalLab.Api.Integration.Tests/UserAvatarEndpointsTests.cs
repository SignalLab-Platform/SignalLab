using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SignalLab.Domain.Files;
using SignalLab.Application.Storage;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class UserAvatarEndpointsTests
    : PlatformEndpointsTestBase, IClassFixture<SignalLabApiFactory>
{
    public UserAvatarEndpointsTests(SignalLabApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CompleteAvatarUpload_WithValidUpload_MarksFileAvailableAndChangesAvatar()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var uploadResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024
            });

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadContent =
            await uploadResponse.Content.ReadAsStringAsync();

        using var uploadDocument =
            JsonDocument.Parse(uploadContent);

        var fileId = uploadDocument.RootElement
            .GetProperty("fileId")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, fileId);

        var storedFileId = new StoredFileId(fileId);

        var pendingStoredFile =
            await factory.GetStoredFileAsync(storedFileId);

        Assert.NotNull(pendingStoredFile);

        Assert.Equal(
            StoredFileStatus.Pending,
            pendingStoredFile.Status);

        using var completeResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            completeResponse.StatusCode);

        var completedStoredFile =
            await factory.GetStoredFileAsync(storedFileId);

        Assert.NotNull(completedStoredFile);

        Assert.Equal(
            StoredFileStatus.Available,
            completedStoredFile.Status);

        var persistedUser =
            await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);

        Assert.Equal(
            storedFileId,
            persistedUser.AvatarFileId);

        Assert.Equal(
            completedStoredFile.ObjectKey,
            factory.LastObjectStorageMetadataKey);
    }

    [Fact]
    public async Task CompleteAvatarUpload_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId = Guid.NewGuid()
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CompleteAvatarUpload_WithMissingObject_DoesNotChangePersistedState()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var uploadResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024
            });

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadContent =
            await uploadResponse.Content.ReadAsStringAsync();

        using var uploadDocument =
            JsonDocument.Parse(uploadContent);

        var fileId = uploadDocument.RootElement
            .GetProperty("fileId")
            .GetGuid();

        factory.ObjectStorageMetadata = null;

        using var completeResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId
            });

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            completeResponse.StatusCode);

        var storedFile = await factory.GetStoredFileAsync(
            new StoredFileId(fileId));

        Assert.NotNull(storedFile);

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Null(persistedUser.AvatarFileId);
    }

    [Fact]
    public async Task CompleteAvatarUpload_WithDifferentContentType_DoesNotChangePersistedState()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var uploadResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024
            });

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadContent =
            await uploadResponse.Content.ReadAsStringAsync();

        using var uploadDocument =
            JsonDocument.Parse(uploadContent);

        var fileId = uploadDocument.RootElement
            .GetProperty("fileId")
            .GetGuid();

        factory.ObjectStorageMetadata = new ObjectMetadata(
            "image/jpeg",
            1024);

        using var completeResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId
            });

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            completeResponse.StatusCode);

        var storedFile = await factory.GetStoredFileAsync(
            new StoredFileId(fileId));

        Assert.NotNull(storedFile);

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Null(persistedUser.AvatarFileId);
    }

    [Fact]
    public async Task CompleteAvatarUpload_WithDifferentSize_DoesNotChangePersistedState()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var uploadResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024
            });

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var uploadContent =
            await uploadResponse.Content.ReadAsStringAsync();

        using var uploadDocument =
            JsonDocument.Parse(uploadContent);

        var fileId = uploadDocument.RootElement
            .GetProperty("fileId")
            .GetGuid();

        factory.ObjectStorageMetadata = new ObjectMetadata(
            "image/png",
            2048);

        using var completeResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId
            });

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            completeResponse.StatusCode);

        var storedFile = await factory.GetStoredFileAsync(
            new StoredFileId(fileId));

        Assert.NotNull(storedFile);

        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Null(persistedUser.AvatarFileId);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithCompletedAvatar_ReturnsAvatarDownloadUrl()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            sessionResponse.StatusCode);

        using var uploadResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024
            });

        Assert.Equal(
            HttpStatusCode.OK,
            uploadResponse.StatusCode);

        var uploadContent =
            await uploadResponse.Content.ReadAsStringAsync();

        using var uploadDocument =
            JsonDocument.Parse(uploadContent);

        var fileId = uploadDocument.RootElement
            .GetProperty("fileId")
            .GetGuid();

        using var completeResponse = await client.PostAsJsonAsync(
            "/users/me/avatar/complete",
            new
            {
                fileId
            });

        Assert.Equal(
            HttpStatusCode.NoContent,
            completeResponse.StatusCode);

        using var profileResponse = await client.GetAsync(
            "/users/me/profile");

        Assert.Equal(
            HttpStatusCode.OK,
            profileResponse.StatusCode);

        var profileContent =
            await profileResponse.Content.ReadAsStringAsync();

        using var profileDocument =
            JsonDocument.Parse(profileContent);

        Assert.Equal(
            fileId,
            profileDocument.RootElement
                .GetProperty("avatarFileId")
                .GetGuid());

        Assert.Equal(
            "https://storage.example.test/download",
            profileDocument.RootElement
                .GetProperty("avatarUrl")
                .GetString());
    }
}