using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationEndpointsTests :
    IClassFixture<SignalLabApiFactory>,
    IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    public OrganizationEndpointsTests(SignalLabApiFactory factory)
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
    public async Task CreateOrganization_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = "https://example.com"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganization_WithAuthenticatedUser_CreatesOrganization()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "  SignalLab Studio  ",
                country = "  FR  ",
                website = "  https://example.com  "
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        var organizationId = root.GetProperty("id").GetGuid();

        Assert.NotEqual(Guid.Empty, organizationId);
        Assert.Equal("SignalLab Studio", root.GetProperty("name").GetString());
        Assert.Equal("FR", root.GetProperty("country").GetString());
        Assert.Equal(
            "https://example.com",
            root.GetProperty("website").GetString());

        Assert.Equal(
            $"/organizations/{organizationId}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("", "FR")]
    [InlineData("SignalLab Studio", "")]
    public async Task CreateOrganization_WithInvalidRequiredFields_ReturnsBadRequest(
        string name,
        string country)
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name,
                country,
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("France")]
    [InlineData("FRA")]
    [InlineData("ZZ")]
    [InlineData("fr")]
    [InlineData("123")]
    public async Task CreateOrganization_WithInvalidCountryCode_ReturnsBadRequest(
        string country)
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country,
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizations_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync("/organizations");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizations_WithoutMemberships_ReturnsEmptyList()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync("/organizations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal(0, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetOrganizations_AfterCreation_ReturnsCreatedOrganization()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var createResponse = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = "https://example.com"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var createdDocument = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync());

        var createdId = createdDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var response = await client.GetAsync("/organizations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var organizations = document.RootElement;

        Assert.Equal(1, organizations.GetArrayLength());

        var organization = organizations[0];

        Assert.Equal(createdId, organization.GetProperty("id").GetGuid());
        Assert.Equal(
            "SignalLab Studio",
            organization.GetProperty("name").GetString());
        Assert.Equal(
            "FR",
            organization.GetProperty("country").GetString());
        Assert.Equal(
            "https://example.com",
            organization.GetProperty("website").GetString());
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetOrganizations_WithMultipleOrganizations_ReturnsAlphabeticalList()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var firstCreation = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "Zeta Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, firstCreation.StatusCode);

        using var secondCreation = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "Alpha Studio",
                country = "BE",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, secondCreation.StatusCode);

        using var response = await client.GetAsync("/organizations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var organizations = document.RootElement;

        Assert.Equal(2, organizations.GetArrayLength());

        Assert.Equal(
            "Alpha Studio",
            organizations[0].GetProperty("name").GetString());

        Assert.Equal(
            "Zeta Studio",
            organizations[1].GetProperty("name").GetString());
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}