using Microsoft.AspNetCore.Mvc.Testing;

namespace SignalLab.Api.Integration.Tests;

public abstract class PlatformEndpointsTestBase : IAsyncLifetime
{
    protected readonly SignalLabApiFactory factory;

    protected PlatformEndpointsTestBase(SignalLabApiFactory factory)
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

    protected HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }
}
