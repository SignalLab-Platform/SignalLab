using System.Net;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class CorsEndpointsTests
    : PlatformEndpointsTestBase, IClassFixture<SignalLabApiFactory>
{
    public CorsEndpointsTests(SignalLabApiFactory factory)
        : base(factory)
    {
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
}
