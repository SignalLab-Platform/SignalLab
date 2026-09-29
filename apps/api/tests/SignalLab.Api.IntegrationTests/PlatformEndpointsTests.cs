using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SignalLab.Api.IntegrationTests;

public sealed class PlatformEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TestPostgresConnectionString =
        "Host=localhost;Port=5432;Database=signallab_tests;Username=signallab;Password=test";

    private readonly WebApplicationFactory<Program> factory;

    public PlatformEndpointsTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting(
                "ConnectionStrings:Postgres",
                TestPostgresConnectionString);
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

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }
}