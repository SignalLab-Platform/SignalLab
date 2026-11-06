using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class AuthenticationEndpointsTests
    : PlatformEndpointsTestBase, IClassFixture<SignalLabApiFactory>
{
    public AuthenticationEndpointsTests(SignalLabApiFactory factory)
        : base(factory)
    {
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
}
