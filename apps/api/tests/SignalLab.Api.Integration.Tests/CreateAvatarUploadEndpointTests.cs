using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Domain.Files;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests;

[Collection(SignalLabApiCollection.Name)]
public sealed class CreateAvatarUploadEndpointTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    public CreateAvatarUploadEndpointTests(SignalLabApiFactory factory)
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
    public async Task CreateAvatarUpload_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024,
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateAvatarUpload_WithValidRequest_ReturnsUploadInformation()
    {
        using var client = CreateClient();

        await AuthenticateAndResolveUserAsync(client);

        using var response = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024,
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<CreateAvatarUploadResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.FileId);
        Assert.Equal(
            "https://storage.example.test/upload",
            result.UploadUrl);
        Assert.True(result.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task CreateAvatarUpload_WithValidRequest_PersistsPendingOwnedStoredFile()
    {
        using var client = CreateClient();

        await AuthenticateAndResolveUserAsync(client);

        var user = await factory.GetTestUserAsync();

        Assert.NotNull(user);

        using var response = await client.PostAsJsonAsync(
            "/users/me/avatar/upload",
            new
            {
                fileName = "avatar.png",
                contentType = "image/png",
                size = 1024,
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<CreateAvatarUploadResponse>();

        Assert.NotNull(result);

        await using var scope = factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var storedFile = await dbContext.StoredFiles
            .AsNoTracking()
            .SingleAsync(
                file => file.Id == new StoredFileId(result.FileId));

        Assert.Equal(user.Id, storedFile.OwnerUserId);
        Assert.Equal("avatar.png", storedFile.OriginalFileName);
        Assert.Equal("image/png", storedFile.ContentType);
        Assert.Equal(1024, storedFile.Size);
        Assert.Equal(
            StoredFileStatus.Pending,
            storedFile.Status);

        Assert.Equal(
            $"avatars/{user.Id.Value:N}/{storedFile.Id.Value:N}",
            storedFile.ObjectKey);
    }

    private async Task AuthenticateAndResolveUserAsync(HttpClient client)
    {
        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        using var response = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    private sealed record CreateAvatarUploadResponse(
        Guid FileId,
        string UploadUrl,
        DateTimeOffset ExpiresAt);
}