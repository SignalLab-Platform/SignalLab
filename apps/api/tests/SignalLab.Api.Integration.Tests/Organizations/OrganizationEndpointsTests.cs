using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalLab.Domain.Organizations;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationEndpointsTests :
    IClassFixture<SignalLabApiFactory>,
    IAsyncLifetime
{
    private const string SecondExternalUserId = "user_test_456";

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

    [Fact]
    public async Task GetOrganization_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganization_WithOwner_ReturnsOrganizationDetails()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var creationResponse = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = "https://example.com"
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var response = await client.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var organization = document.RootElement;

        Assert.Equal(
            organizationId,
            organization.GetProperty("id").GetGuid());

        Assert.Equal(
            "SignalLab Studio",
            organization.GetProperty("name").GetString());

        Assert.Equal(
            "FR",
            organization.GetProperty("country").GetString());

        Assert.Equal(
            "https://example.com",
            organization.GetProperty("website").GetString());

        Assert.True(
            organization.GetProperty("createdAt").GetDateTimeOffset() <=
            organization.GetProperty("updatedAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task GetOrganization_WithUnknownId_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganization_WithInvalidId_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            "/organizations/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganization_WithExternalUser_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSessionResponse = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSessionResponse.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var externalClient = CreateSecondAuthenticatedClient();

        using var externalSessionResponse = await externalClient.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            externalSessionResponse.StatusCode);

        var secondUser = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(secondUser);

        using var response = await externalClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var ownerResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
    }

    [Fact]
    public async Task GetOrganization_WithActiveMember_ReturnsOk()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSessionResponse = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSessionResponse.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSessionResponse = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSessionResponse.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        using var response = await memberClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganization_WithRemovedMember_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSessionResponse = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSessionResponse.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSessionResponse = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSessionResponse.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        var domainOrganizationId = new OrganizationId(organizationId);

        await factory.Database.AddOrganizationMemberAsync(
            domainOrganizationId,
            member.Id,
            "Member");

        using var activeResponse = await memberClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);

        await factory.Database.RemoveOrganizationMemberAsync(
            domainOrganizationId,
            member.Id);

        using var removedResponse = await memberClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.NotFound, removedResponse.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}/members");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithUnknownOrganization_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}/members");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithExternalUser_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var externalClient = CreateSecondAuthenticatedClient();

        using var externalSession = await externalClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, externalSession.StatusCode);

        using var response = await externalClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithRemovedMember_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSession = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSession.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        using var beforeRemoval = await memberClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, beforeRemoval.StatusCode);

        await factory.Database.RemoveOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id);

        using var afterRemoval = await memberClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.NotFound, afterRemoval.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithOwner_ReturnsOwnerDetails()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        var owner = await factory.Database.GetUserAsync(
            SignalLabApiFactory.TestExternalUserId);

        Assert.NotNull(owner);

        using var creationResponse = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var response = await client.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var members = document.RootElement;

        Assert.Equal(JsonValueKind.Array, members.ValueKind);
        Assert.Equal(1, members.GetArrayLength());

        var member = members[0];

        Assert.NotEqual(
            Guid.Empty,
            member.GetProperty("id").GetGuid());

        Assert.Equal(
            owner.Id.Value,
            member.GetProperty("userId").GetGuid());

        Assert.Equal(
            owner.Email,
            member.GetProperty("email").GetString());

        Assert.Equal(
            "Owner",
            member.GetProperty("roleName").GetString());

        Assert.NotEqual(
            Guid.Empty,
            member.GetProperty("roleId").GetGuid());

        Assert.Equal(
            "Active",
            member.GetProperty("status").GetString());

        Assert.NotEqual(
            default(DateTimeOffset),
            member.GetProperty("joinedAt").GetDateTimeOffset());

        Assert.Equal(
            JsonValueKind.Null,
            member.GetProperty("removedAt").ValueKind);
    }

    [Fact]
    public async Task GetOrganizationMembers_WithActiveMemberWithoutManageMembers_ReturnsOk()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSession = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSession.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        using var response = await memberClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var members = document.RootElement;

        Assert.Equal(JsonValueKind.Array, members.ValueKind);
        Assert.Equal(2, members.GetArrayLength());

        var regularMember = Assert.Single(
            members.EnumerateArray(),
            item => item.GetProperty("userId").GetGuid() == member.Id.Value);

        Assert.Equal(
            "Member",
            regularMember.GetProperty("roleName").GetString());

        Assert.Equal(
            "Active",
            regularMember.GetProperty("status").GetString());

        Assert.Equal(
            "second-user@example.com",
            regularMember.GetProperty("email").GetString());

        Assert.Contains(
            members.EnumerateArray(),
            item => item.GetProperty("roleName").GetString() == "Owner");
    }

    [Fact]
    public async Task GetOrganizationMembers_WithRemovedMember_IncludesMembershipHistory()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSession = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSession.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        await factory.Database.RemoveOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id);

        using var response = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var members = document.RootElement;

        Assert.Equal(JsonValueKind.Array, members.ValueKind);
        Assert.Equal(2, members.GetArrayLength());

        var removedMember = Assert.Single(
            members.EnumerateArray(),
            item => item.GetProperty("userId").GetGuid() == member.Id.Value);

        Assert.Equal(
            "Removed",
            removedMember.GetProperty("status").GetString());

        Assert.Equal(
            "Member",
            removedMember.GetProperty("roleName").GetString());

        var joinedAt = removedMember
            .GetProperty("joinedAt")
            .GetDateTimeOffset();

        var removedAt = removedMember
            .GetProperty("removedAt")
            .GetDateTimeOffset();

        Assert.True(removedAt >= joinedAt);

        Assert.Equal(
            "Active",
            members[0].GetProperty("status").GetString());

        Assert.Equal(
            "Removed",
            members[1].GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetOrganizationRoles_WithoutToken_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}/roles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationRoles_WithUnknownOrganization_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}/roles");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationRoles_WithExternalUser_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var externalClient = CreateSecondAuthenticatedClient();

        using var externalSession = await externalClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, externalSession.StatusCode);

        using var response = await externalClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationRoles_WithRemovedMember_ReturnsNotFound()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSession = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSession.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        using var beforeRemoval = await memberClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, beforeRemoval.StatusCode);

        await factory.Database.RemoveOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id);

        using var afterRemoval = await memberClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.NotFound, afterRemoval.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationRoles_WithActiveMemberWithoutManageRoles_ReturnsOk()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var creationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var memberClient = CreateSecondAuthenticatedClient();

        using var memberSession = await memberClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, memberSession.StatusCode);

        var member = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(member);

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(organizationId),
            member.Id,
            "Member");

        using var response = await memberClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var roles = document.RootElement;

        Assert.Equal(JsonValueKind.Array, roles.ValueKind);
        Assert.Equal(3, roles.GetArrayLength());

        Assert.Contains(
            roles.EnumerateArray(),
            role => role.GetProperty("isOwner").GetBoolean());
    }

    [Fact]
    public async Task GetOrganizationRoles_WithOwner_ReturnsRoleDetails()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var creationResponse = await client.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var organizationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var response = await client.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var roles = document.RootElement;

        Assert.Equal(JsonValueKind.Array, roles.ValueKind);
        Assert.Equal(3, roles.GetArrayLength());

        var ownerRole = Assert.Single(
            roles.EnumerateArray(),
            role => role.GetProperty("isOwner").GetBoolean());

        Assert.NotEqual(
            Guid.Empty,
            ownerRole.GetProperty("id").GetGuid());

        Assert.Equal(
            "Owner",
            ownerRole.GetProperty("name").GetString());

        Assert.False(
            string.IsNullOrWhiteSpace(
                ownerRole.GetProperty("description").GetString()));

        Assert.NotEqual(
            default(DateTimeOffset),
            ownerRole.GetProperty("createdAt").GetDateTimeOffset());

        Assert.NotEqual(
            default(DateTimeOffset),
            ownerRole.GetProperty("updatedAt").GetDateTimeOffset());

        var permissions = ownerRole
            .GetProperty("permissions")
            .EnumerateArray()
            .Select(permission => permission.GetString())
            .ToArray();

        Assert.Equal(
            Enum.GetNames<Permission>().Length,
            permissions.Length);

        Assert.Contains("ManageOwnership", permissions);
        Assert.Contains("ManageMembers", permissions);
        Assert.Contains("ManageOrganization", permissions);

        Assert.Equal(
            permissions.Length,
            permissions.Distinct().Count());

        Assert.Equal(
            "Owner",
            roles[0].GetProperty("name").GetString());
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

    private HttpClient CreateSecondAuthenticatedClient()
    {
        var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SecondExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}