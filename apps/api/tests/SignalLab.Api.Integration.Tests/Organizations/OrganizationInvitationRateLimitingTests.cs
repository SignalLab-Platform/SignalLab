using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationInvitationRateLimitingTests : IAsyncLifetime
{
    private SignalLabApiFactory? factory;
    private readonly List<OrganizationId> createdOrganizations = new();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (factory is null)
        {
            return;
        }

        try
        {
            await factory.Database.ResetOrganizationsAsync(createdOrganizations);
            await factory.ResetTestUserAsync();
        }
        finally
        {
            factory.Dispose();
        }
    }

    [Fact]
    public async Task UserQuota_ShouldRejectThirdAttempt()
    {
        await StartAsync(userLimit: 2, organizationLimit: 10);

        using var ownerClient = await CreateOwnerClientAsync();
        var organizationId = await CreateOrganizationAsync(ownerClient);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var response = await SendInvitationAsync(
                ownerClient,
                organizationId,
                "missing@example.com");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var rejectedResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "another-missing@example.com");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            rejectedResponse.StatusCode);
    }

    [Fact]
    public async Task UserQuota_ShouldCountSuccessfulAndFailedRequests()
    {
        await StartAsync(userLimit: 2, organizationLimit: 10);

        using var ownerClient = await CreateOwnerClientAsync();
        var organizationId = await CreateOrganizationAsync(ownerClient);

        using var recipientClient = await CreateSecondUserClientAsync();

        var memberRoleId = await GetRoleIdAsync(
            ownerClient,
            organizationId,
            "Member");

        using var createdResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "second-user@example.com",
            memberRoleId);

        Assert.Equal(
            HttpStatusCode.Created,
            createdResponse.StatusCode);

        using var failedResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing@example.com",
            memberRoleId);

        Assert.Equal(
            HttpStatusCode.NotFound,
            failedResponse.StatusCode);

        using var rejectedResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "another-missing@example.com",
            memberRoleId);

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            rejectedResponse.StatusCode);
    }

    [Fact]
    public async Task OrganizationQuota_ShouldBeSharedBetweenMembers()
    {
        await StartAsync(userLimit: 10, organizationLimit: 3);

        using var ownerClient = await CreateOwnerClientAsync();
        var organizationId = await CreateOrganizationAsync(ownerClient);

        using var administratorClient = await CreateSecondUserClientAsync();

        var administrator = await factory!.Database.GetUserAsync(
            "user_test_456");

        Assert.NotNull(administrator);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            administrator.Id,
            "Administrator");

        using var firstResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing-1@example.com");

        using var secondResponse = await SendInvitationAsync(
            administratorClient,
            organizationId,
            "missing-2@example.com");

        using var thirdResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing-3@example.com");

        Assert.Equal(HttpStatusCode.NotFound, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, thirdResponse.StatusCode);

        using var rejectedResponse = await SendInvitationAsync(
            administratorClient,
            organizationId,
            "missing-4@example.com");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            rejectedResponse.StatusCode);
    }

    [Fact]
    public async Task OrganizationQuota_ShouldBeIndependentAcrossOrganizations()
    {
        await StartAsync(userLimit: 10, organizationLimit: 2);

        using var ownerClient = await CreateOwnerClientAsync();

        var firstOrganizationId = await CreateOrganizationAsync(ownerClient);
        var secondOrganizationId = await CreateOrganizationAsync(ownerClient);

        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var response = await SendInvitationAsync(
                ownerClient,
                firstOrganizationId,
                "missing@example.com");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var rejectedResponse = await SendInvitationAsync(
            ownerClient,
            firstOrganizationId,
            "missing@example.com");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            rejectedResponse.StatusCode);

        using var otherOrganizationResponse = await SendInvitationAsync(
            ownerClient,
            secondOrganizationId,
            "missing@example.com");

        Assert.Equal(
            HttpStatusCode.NotFound,
            otherOrganizationResponse.StatusCode);
    }

    [Fact]
    public async Task UserQuota_ShouldBeIndependentBetweenUsers()
    {
        await StartAsync(userLimit: 2, organizationLimit: 10);

        using var ownerClient = await CreateOwnerClientAsync();
        var organizationId = await CreateOrganizationAsync(ownerClient);

        using var administratorClient = await CreateSecondUserClientAsync();

        var administrator = await factory!.Database.GetUserAsync(
            "user_test_456");

        Assert.NotNull(administrator);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            administrator.Id,
            "Administrator");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var response = await SendInvitationAsync(
                ownerClient,
                organizationId,
                "missing@example.com");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        using var ownerRejectedResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing@example.com");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            ownerRejectedResponse.StatusCode);

        using var administratorResponse = await SendInvitationAsync(
            administratorClient,
            organizationId,
            "missing@example.com");

        Assert.Equal(
            HttpStatusCode.NotFound,
            administratorResponse.StatusCode);
    }

    [Fact]
    public async Task ExhaustedQuota_ShouldNotBlockOtherInvitationEndpoints()
    {
        await StartAsync(userLimit: 1, organizationLimit: 1);

        using var ownerClient = await CreateOwnerClientAsync();
        var organizationId = await CreateOrganizationAsync(ownerClient);

        using var firstResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing@example.com");

        Assert.Equal(HttpStatusCode.NotFound, firstResponse.StatusCode);

        using var rejectedResponse = await SendInvitationAsync(
            ownerClient,
            organizationId,
            "missing@example.com");

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            rejectedResponse.StatusCode);

        using var listResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/invitations");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var personalResponse = await ownerClient.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(HttpStatusCode.OK, personalResponse.StatusCode);

        using var acceptResponse = await ownerClient.PostAsync(
            $"/organization-invitations/{Guid.NewGuid()}/accept",
            null);

        Assert.Equal(HttpStatusCode.NotFound, acceptResponse.StatusCode);
    }

    private async Task StartAsync(int userLimit, int organizationLimit)
    {
        factory = new SignalLabApiFactory(userLimit, organizationLimit);

        await factory.InitializeDatabaseAsync();
        await factory.ResetTestUserAsync();
    }

    private HttpClient CreateAuthenticatedClient(string externalUserId)
    {
        var client = factory!.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            externalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private async Task<HttpClient> CreateOwnerClientAsync()
    {
        var client = CreateAuthenticatedClient(
            SignalLabApiFactory.TestExternalUserId);

        using var response = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return client;
    }

    private async Task<HttpClient> CreateSecondUserClientAsync()
    {
        factory!.ExternalIdentityUserId = "user_test_456";
        factory.ExternalIdentityEmail = "second-user@example.com";

        var client = CreateAuthenticatedClient("user_test_456");

        using var response = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return client;
    }

    private async Task<Guid> CreateOrganizationAsync(HttpClient ownerClient)
    {
        using var response = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "Rate Limiting Test Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var organizationId = document.RootElement
            .GetProperty("id")
            .GetGuid();

        createdOrganizations.Add(new OrganizationId(organizationId));

        return organizationId;
    }

    private static async Task<Guid> GetRoleIdAsync(
        HttpClient client,
        Guid organizationId,
        string roleName)
    {
        using var response = await client.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return document.RootElement
            .EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == roleName)
            .GetProperty("id")
            .GetGuid();
    }

    private static Task<HttpResponseMessage> SendInvitationAsync(
        HttpClient client,
        Guid organizationId,
        string email,
        Guid? roleId = null)
    {
        return client.PostAsJsonAsync(
            $"/organizations/{organizationId}/invitations",
            new
            {
                email,
                roleId = roleId ?? Guid.NewGuid(),
                duration = 7
            });
    }
}