using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class UserProfileEndpointsTests
    : PlatformEndpointsTestBase, IClassFixture<SignalLabApiFactory>
{
    public UserProfileEndpointsTests(SignalLabApiFactory factory)
        : base(factory)
    {
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
}
