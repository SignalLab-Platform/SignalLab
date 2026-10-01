using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SignalLab.Api.IntegrationTests;

public sealed class PlatformEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TestPostgresConnectionString =
        "Host=localhost;Port=5432;Database=signallab_tests;Username=signallab;Password=test";

    private readonly WebApplicationFactory<Program> factory;

    private const string TestIssuer = "https://clerk.example.test";
    private const string TestAuthorizedParty = "https://web.example.test";
    private const string TestAllowedOrigin = "https://web.example.test";
    private const string TestExternalUserId = "user_test_123";

    private readonly TestJwtTokenFactory tokenFactory = new();

    public PlatformEndpointsTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");

            builder.UseSetting(
                "ConnectionStrings:Postgres",
                TestPostgresConnectionString);

            builder.UseSetting(
                "Authentication:Clerk:Issuer",
                TestIssuer);

            builder.UseSetting(
                "Authentication:Clerk:AuthorizedParties:0",
                TestAuthorizedParty);

            builder.UseSetting(
                "Cors:AllowedOrigins:0",
                TestAllowedOrigin);

            builder.ConfigureServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Authority = null;
                        options.ConfigurationManager = null;

                        options.TokenValidationParameters.IssuerSigningKey =
                            tokenFactory.ValidationKey;
                    });
            });
        });
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
    public async Task AuthenticationSession_WithValidToken_ReturnsExternalUserId()
    {
        using var client = CreateClient();

        var token = tokenFactory.CreateToken(
            TestIssuer,
            TestAuthorizedParty,
            TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        var externalUserId = document.RootElement
            .GetProperty("externalUserId")
            .GetString();

        Assert.Equal(TestExternalUserId, externalUserId);
    }

    [Fact]
    public async Task AuthenticationSession_WithInvalidIssuer_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = tokenFactory.CreateToken(
            "https://invalid-issuer.example.test",
            TestAuthorizedParty,
            TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithUnauthorizedParty_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = tokenFactory.CreateToken(
            TestIssuer,
            "https://unauthorized-web.example.test",
            TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticationSession_WithExpiredToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var token = tokenFactory.CreateToken(
            TestIssuer,
            TestAuthorizedParty,
            TestExternalUserId,
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
            TestIssuer,
            TestAuthorizedParty,
            TestExternalUserId);

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
            TestAllowedOrigin);

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
            TestAllowedOrigin,
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