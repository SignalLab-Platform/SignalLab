using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalLab.Domain.Files;
using SignalLab.Application.Storage;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class PlatformEndpointsTests : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    public PlatformEndpointsTests(SignalLabApiFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        await factory.ResetTestUserAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Health_ReturnsHealthyStatus()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task OpenApi_ContainsHealthEndpoint()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(content);
        var paths = document.RootElement.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/health", out _));
    }

    [Fact]
    public async Task AuthenticationSession_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithInvalidToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this-is-not-a-valid-jwt");

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithValidToken_ReturnsResolvedUser()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        var userId = document.RootElement
            .GetProperty("id")
            .GetGuid();

        var email = document.RootElement
            .GetProperty("email")
            .GetString();

        Assert.NotEqual(Guid.Empty, userId);
        Assert.Equal(SignalLabApiFactory.TestEmail, email);

        using var secondResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var secondContent =
            await secondResponse.Content.ReadAsStringAsync();

        using var secondDocument =
            JsonDocument.Parse(secondContent);

        var secondUserId = secondDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        var secondEmail = secondDocument.RootElement
            .GetProperty("email")
            .GetString();

        Assert.Equal(userId, secondUserId);
        Assert.Equal(
            SignalLabApiFactory.TestEmail,
            secondEmail);
        Assert.Equal(
            1,
            factory.ExternalIdentityProviderCallCount);
    }

    [Fact]
    public async Task AuthenticationSession_WithInvalidIssuer_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            "https://invalid-issuer.example.test",
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithUnauthorizedParty_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            "https://unauthorized-web.example.test",
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithExpiredToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId,
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: DateTime.UtcNow.AddMinutes(-5));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithInvalidSignature_ReturnsUnauthorized()
    {
        using var client = CreateClient();
        using var unauthorizedTokenFactory = new TestJwtTokenFactory();

        var token = unauthorizedTokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_PreflightFromAllowedOrigin_AllowsCors()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/authentication/session");

        request.Headers.Add(
            "Origin",
            SignalLabApiFactory.TestAllowedOrigin);

        request.Headers.Add(
            "Access-Control-Request-Method",
            "GET");

        request.Headers.Add(
            "Access-Control-Request-Headers",
            "authorization");

        using var response = await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        Assert.Equal(
            SignalLabApiFactory.TestAllowedOrigin,
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());

        var allowedHeaders = response.Headers
            .GetValues("Access-Control-Allow-Headers")
            .SelectMany(value => value.Split(','))
            .Select(value => value.Trim());

        Assert.Contains(
            allowedHeaders,
            header => string.Equals(
                header,
                "authorization",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AuthenticationSession_PreflightFromUnknownOrigin_DoesNotAllowCors()
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Options,
            "/authentication/session");

        request.Headers.Add(
            "Origin",
            "https://evil.example.test");

        request.Headers.Add(
            "Access-Control-Request-Method",
            "GET");

        request.Headers.Add(
            "Access-Control-Request-Headers",
            "authorization");

        using var response = await client.SendAsync(request);

        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            "/users/me/profile");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithResolvedUser_ReturnsPersistedProfile()
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

        using var firstNameResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/first-name",
            new
            {
                firstName = "Jimmy"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            firstNameResponse.StatusCode);

        using var lastNameResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/last-name",
            new
            {
                lastName = "Fromonot"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            lastNameResponse.StatusCode);

        using var response = await client.GetAsync(
            "/users/me/profile");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        var root = document.RootElement;

        Assert.Equal(
            SignalLabApiFactory.TestEmail,
            root.GetProperty("email").GetString());

        Assert.Equal(
            "Jimmy",
            root.GetProperty("firstName").GetString());

        Assert.Equal(
            "Fromonot",
            root.GetProperty("lastName").GetString());

        Assert.Equal(
            "English",
            root.GetProperty("language").GetString());

        Assert.Equal(
            JsonValueKind.Null,
            root.GetProperty("avatarFileId").ValueKind);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);

        Assert.Equal(
            persistedUser.Id.Value,
            root.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task ChangeFirstName_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PatchAsJsonAsync(
            "/users/me/profile/first-name",
            new
            {
                firstName = "Jimmy"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeFirstName_WithResolvedUser_ChangesPersistedFirstName()
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

        using var response = await client.PatchAsJsonAsync(
            "/users/me/profile/first-name",
            new
            {
                firstName = "  Jimmy  "
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(
            "Jimmy",
            document.RootElement
                .GetProperty("firstName")
                .GetString());

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Equal("Jimmy", persistedUser.FirstName);
    }

    [Fact]
    public async Task ChangeFirstName_WithEmptyValue_ClearsFirstName()
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

        using var firstResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/first-name",
            new
            {
                firstName = "Jimmy"
            });

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var clearResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/first-name",
            new
            {
                firstName = "   "
            });

        Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);

        var content = await clearResponse.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(
            JsonValueKind.Null,
            document.RootElement
                .GetProperty("firstName")
                .ValueKind);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Null(persistedUser.FirstName);
    }

    [Fact]
    public async Task ChangeLastName_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PatchAsJsonAsync(
            "/users/me/profile/last-name",
            new
            {
                lastName = "Fromonot"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeLastName_WithResolvedUser_ChangesPersistedLastName()
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

        using var response = await client.PatchAsJsonAsync(
            "/users/me/profile/last-name",
            new
            {
                lastName = "  Fromonot  "
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(
            "Fromonot",
            document.RootElement
                .GetProperty("lastName")
                .GetString());

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Equal("Fromonot", persistedUser.LastName);
    }

    [Fact]
    public async Task ChangeLastName_WithEmptyValue_ClearsLastName()
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

        using var firstResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/last-name",
            new
            {
                lastName = "Fromonot"
            });

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var clearResponse = await client.PatchAsJsonAsync(
            "/users/me/profile/last-name",
            new
            {
                lastName = "   "
            });

        Assert.Equal(HttpStatusCode.OK, clearResponse.StatusCode);

        var content = await clearResponse.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(
            JsonValueKind.Null,
            document.RootElement
                .GetProperty("lastName")
                .ValueKind);

        var persistedUser = await factory.GetTestUserAsync();

        Assert.NotNull(persistedUser);
        Assert.Null(persistedUser.LastName);
    }

    [Fact]
    public async Task SynchronizeEmail_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(
            "/users/me/profile/email/synchronize",
            null);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task SynchronizeEmail_WithChangedExternalEmail_ChangesPersistedEmail()
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

        var persistedUserBeforeSynchronization =
            await factory.GetTestUserAsync();

        Assert.NotNull(persistedUserBeforeSynchronization);

        Assert.Equal(
            SignalLabApiFactory.TestEmail,
            persistedUserBeforeSynchronization.Email);

        factory.ExternalIdentityEmail = "new@example.com";

        using var response = await client.PostAsync(
            "/users/me/profile/email/synchronize",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        Assert.Equal(
            "new@example.com",
            document.RootElement
                .GetProperty("email")
                .GetString());

        var persistedUserAfterSynchronization =
            await factory.GetTestUserAsync();

        Assert.NotNull(persistedUserAfterSynchronization);

        Assert.Equal(
            "new@example.com",
            persistedUserAfterSynchronization.Email);
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

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }
}