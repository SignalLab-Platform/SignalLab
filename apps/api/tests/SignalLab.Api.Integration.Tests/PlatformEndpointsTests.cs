using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SignalLab.Api.Integration.Tests;

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
            .GetProperty("userId")
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
            .GetProperty("userId")
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

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }
}