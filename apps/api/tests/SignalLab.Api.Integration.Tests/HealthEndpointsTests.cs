using System.Net;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class HealthEndpointsTests
    : PlatformEndpointsTestBase, IClassFixture<SignalLabApiFactory>
{
    public HealthEndpointsTests(SignalLabApiFactory factory)
        : base(factory)
    {
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
}
