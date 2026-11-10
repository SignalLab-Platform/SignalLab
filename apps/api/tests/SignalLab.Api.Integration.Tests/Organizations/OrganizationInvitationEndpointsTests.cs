using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Organizations;
using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationInvitationEndpointsTests :
    IClassFixture<SignalLabApiFactory>,
    IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    public OrganizationInvitationEndpointsTests(
        SignalLabApiFactory factory)
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

    [Theory]
    [InlineData("/organizations/00000000-0000-0000-0000-000000000001/invitations")]
    [InlineData("/organization-invitations/00000000-0000-0000-0000-000000000001")]
    [InlineData("/users/me/organization-invitations")]
    public async Task GetInvitations_WithoutToken_ReturnsUnauthorized(
        string path)
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserInvitations_WithoutSession_ReturnsConflict()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "session_initialization_required",
            document.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetCurrentUserInvitations_WithoutInvitations_ReturnsEmptyList()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            JsonValueKind.Array,
            document.RootElement.ValueKind);

        Assert.Equal(
            0,
            document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetOrganizationInvitations_WithUnknownOrganization_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            $"/organizations/{Guid.NewGuid()}/invitations");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetOrganizationInvitation_WithUnknownInvitation_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(
            HttpStatusCode.OK,
            sessionResponse.StatusCode);

        using var response = await client.GetAsync(
            $"/organization-invitations/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateAndAcceptInvitation_ActivatesMembership()
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var organizationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.Created,
            organizationResponse.StatusCode);

        using var organizationDocument = JsonDocument.Parse(
            await organizationResponse.Content.ReadAsStringAsync());

        var organizationId = organizationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var rolesResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

        using var rolesDocument = JsonDocument.Parse(
            await rolesResponse.Content.ReadAsStringAsync());

        var memberRole = rolesDocument.RootElement
            .EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == "Member");

        var roleId = memberRole.GetProperty("id").GetGuid();

        const string secondExternalUserId = "user_test_456";

        factory.ExternalIdentityUserId = secondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var invitedClient = CreateSecondAuthenticatedClient(
            secondExternalUserId);

        using var invitedSession = await invitedClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, invitedSession.StatusCode);

        var invitedUser = await factory.Database.GetUserAsync(
            secondExternalUserId);

        Assert.NotNull(invitedUser);

        using var invitationResponse = await ownerClient.PostAsJsonAsync(
            $"/organizations/{organizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId,
                duration = 7
            });

        Assert.Equal(
            HttpStatusCode.Created,
            invitationResponse.StatusCode);

        using var invitationDocument = JsonDocument.Parse(
            await invitationResponse.Content.ReadAsStringAsync());

        var invitationId = invitationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.Equal(
            "Pending",
            invitationDocument.RootElement.GetProperty("status").GetString());

        Assert.Equal(
            $"/organization-invitations/{invitationId}",
            invitationResponse.Headers.Location?.OriginalString);

        using var invitationsResponse = await invitedClient.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(HttpStatusCode.OK, invitationsResponse.StatusCode);

        using var invitationsDocument = JsonDocument.Parse(
            await invitationsResponse.Content.ReadAsStringAsync());

        var invitations = invitationsDocument.RootElement;

        Assert.Equal(1, invitations.GetArrayLength());

        Assert.Equal(
            invitationId,
            invitations[0].GetProperty("id").GetGuid());

        Assert.Equal(
            "SignalLab Studio",
            invitations[0].GetProperty("organizationName").GetString());

        using var acceptResponse = await invitedClient.PostAsync(
            $"/organization-invitations/{invitationId}/accept",
            null);

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

        using var acceptedDocument = JsonDocument.Parse(
            await acceptResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Accepted",
            acceptedDocument.RootElement.GetProperty("status").GetString());

        using var membershipResponse = await invitedClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.OK, membershipResponse.StatusCode);

        using var membersResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, membersResponse.StatusCode);

        using var membersDocument = JsonDocument.Parse(
            await membersResponse.Content.ReadAsStringAsync());

        Assert.Contains(
            membersDocument.RootElement.EnumerateArray(),
            member =>
                member.GetProperty("userId").GetGuid() == invitedUser.Id.Value &&
                member.GetProperty("status").GetString() == "Active" &&
                member.GetProperty("roleName").GetString() == "Member");
    }

    [Theory]
    [InlineData("decline", "Declined")]
    [InlineData("cancel", "Cancelled")]
    public async Task CompleteInvitationWithoutAcceptance_DoesNotCreateMembership(
        string action,
        string expectedStatus)
    {
        using var ownerClient = CreateAuthenticatedClient();

        using var ownerSession = await ownerClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

        using var organizationResponse = await ownerClient.PostAsJsonAsync(
            "/organizations",
            new
            {
                name = "SignalLab Studio",
                country = "FR",
                website = (string?)null
            });

        Assert.Equal(HttpStatusCode.Created, organizationResponse.StatusCode);

        using var organizationDocument = JsonDocument.Parse(
            await organizationResponse.Content.ReadAsStringAsync());

        var organizationId = organizationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var rolesResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

        using var rolesDocument = JsonDocument.Parse(
            await rolesResponse.Content.ReadAsStringAsync());

        var roleId = rolesDocument.RootElement
            .EnumerateArray()
            .Single(role => role.GetProperty("name").GetString() == "Member")
            .GetProperty("id")
            .GetGuid();

        const string secondExternalUserId = "user_test_456";

        factory.ExternalIdentityUserId = secondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var invitedClient = CreateSecondAuthenticatedClient(
            secondExternalUserId);

        using var invitedSession = await invitedClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, invitedSession.StatusCode);

        var invitedUser = await factory.Database.GetUserAsync(
            secondExternalUserId);

        Assert.NotNull(invitedUser);

        using var invitationResponse = await ownerClient.PostAsJsonAsync(
            $"/organizations/{organizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId,
                duration = 7
            });

        Assert.Equal(HttpStatusCode.Created, invitationResponse.StatusCode);

        using var invitationDocument = JsonDocument.Parse(
            await invitationResponse.Content.ReadAsStringAsync());

        var invitationId = invitationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        var actingClient = action == "decline"
            ? invitedClient
            : ownerClient;

        using var actionResponse = await actingClient.PostAsync(
            $"/organization-invitations/{invitationId}/{action}",
            null);

        Assert.Equal(HttpStatusCode.OK, actionResponse.StatusCode);

        using var actionDocument = JsonDocument.Parse(
            await actionResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            expectedStatus,
            actionDocument.RootElement.GetProperty("status").GetString());

        using var invitationDetailsResponse = await invitedClient.GetAsync(
            $"/organization-invitations/{invitationId}");

        Assert.Equal(HttpStatusCode.OK, invitationDetailsResponse.StatusCode);

        using var detailsDocument = JsonDocument.Parse(
            await invitationDetailsResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            expectedStatus,
            detailsDocument.RootElement.GetProperty("status").GetString());

        using var organizationAccessResponse = await invitedClient.GetAsync(
            $"/organizations/{organizationId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            organizationAccessResponse.StatusCode);

        using var membersResponse = await ownerClient.GetAsync(
            $"/organizations/{organizationId}/members");

        Assert.Equal(HttpStatusCode.OK, membersResponse.StatusCode);

        using var membersDocument = JsonDocument.Parse(
            await membersResponse.Content.ReadAsStringAsync());

        Assert.DoesNotContain(
            membersDocument.RootElement.EnumerateArray(),
            member =>
                member.GetProperty("userId").GetGuid() == invitedUser.Id.Value &&
                member.GetProperty("status").GetString() == "Active");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(-1)]
    public async Task CreateInvitation_WithInvalidDuration_ReturnsBadRequest(
        int duration)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = scenario.MemberRoleId,
                duration
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.BadRequest,
            OrganizationInvitationErrorCodes.InvalidDuration);
    }

    [Fact]
    public async Task CreateInvitation_WithRoleFromAnotherOrganization_ReturnsBadRequest()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var secondOrganizationResponse =
            await scenario.OwnerClient.PostAsJsonAsync(
                "/organizations",
                new
                {
                    name = "Second Studio",
                    country = "BE",
                    website = (string?)null
                });

        Assert.Equal(
            HttpStatusCode.Created,
            secondOrganizationResponse.StatusCode);

        using var secondOrganizationDocument = JsonDocument.Parse(
            await secondOrganizationResponse.Content.ReadAsStringAsync());

        var secondOrganizationId = secondOrganizationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var rolesResponse = await scenario.OwnerClient.GetAsync(
            $"/organizations/{secondOrganizationId}/roles");

        Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

        using var rolesDocument = JsonDocument.Parse(
            await rolesResponse.Content.ReadAsStringAsync());

        var foreignRoleId = rolesDocument.RootElement
            .EnumerateArray()
            .Single(role =>
                role.GetProperty("name").GetString() == "Member")
            .GetProperty("id")
            .GetGuid();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = foreignRoleId,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.BadRequest,
            OrganizationInvitationErrorCodes.InvalidRole);
    }

    [Fact]
    public async Task CreateInvitation_WithoutManageMembers_ReturnsForbidden()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        await factory.Database.AddOrganizationMemberAsync(
            new OrganizationId(scenario.OrganizationId),
            new SignalLab.Domain.Users.UserId(scenario.InvitedUserId),
            "Member");

        using var response = await scenario.InvitedClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.Forbidden,
            OrganizationInvitationErrorCodes.ManageMembersDenied);
    }

    [Fact]
    public async Task CreateInvitation_AsExternalUser_ReturnsNotFound()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.InvitedClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.NotFound,
            OrganizationInvitationErrorCodes.OrganizationNotFound);
    }

    [Theory]
    [InlineData("SECOND-USER@EXAMPLE.COM")]
    [InlineData("  second-user@example.com  ")]
    public async Task CreateInvitation_WithNormalizedEmail_Succeeds(
        string email)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email,
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ChangeUnknownInvitation_ReturnsNotFound(
        string action)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsync(
            $"/organization-invitations/{Guid.NewGuid()}/{action}",
            null);

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.NotFound,
            OrganizationInvitationErrorCodes.InvitationNotFound);
    }

    [Fact]
    public async Task CreateInvitation_WhenAlreadyPending_ReturnsConflict()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        var request = new
        {
            email = "second-user@example.com",
            roleId = scenario.MemberRoleId,
            duration = 7
        };

        using var firstResponse = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var secondResponse = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            request);

        await AssertApplicationErrorAsync(
            secondResponse,
            HttpStatusCode.Conflict,
            OrganizationInvitationErrorCodes.AlreadyPending);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ChangeCompletedInvitation_ReturnsConflict(
        string action)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var creationResponse =
            await scenario.OwnerClient.PostAsJsonAsync(
                $"/organizations/{scenario.OrganizationId}/invitations",
                new
                {
                    email = "second-user@example.com",
                    roleId = scenario.MemberRoleId,
                    duration = 7
                });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var invitationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        var actingClient = action == "cancel"
            ? scenario.OwnerClient
            : scenario.InvitedClient;

        using var firstResponse = await actingClient.PostAsync(
            $"/organization-invitations/{invitationId}/{action}",
            null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        using var secondResponse = await actingClient.PostAsync(
            $"/organization-invitations/{invitationId}/{action}",
            null);

        await AssertApplicationErrorAsync(
            secondResponse,
            HttpStatusCode.Conflict,
            OrganizationInvitationErrorCodes.InvalidState);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("decline")]
    public async Task RespondToAnotherUsersInvitation_ReturnsNotFound(
        string action)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var creationResponse =
            await scenario.OwnerClient.PostAsJsonAsync(
                $"/organizations/{scenario.OrganizationId}/invitations",
                new
                {
                    email = "second-user@example.com",
                    roleId = scenario.MemberRoleId,
                    duration = 7
                });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var invitationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var response = await scenario.OwnerClient.PostAsync(
            $"/organization-invitations/{invitationId}/{action}",
            null);

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.NotFound,
            OrganizationInvitationErrorCodes.InvitationNotFound);
    }

    [Theory]
    [InlineData("/organizations/00000000-0000-0000-0000-000000000000/invitations")]
    [InlineData("/organization-invitations/00000000-0000-0000-0000-000000000000")]
    [InlineData("/organization-invitations/not-a-guid")]
    public async Task GetInvitations_WithInvalidResourceId_ReturnsNotFound(
        string path)
    {
        using var client = CreateAuthenticatedClient();

        using var sessionResponse = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ChangeInvitation_WithEmptyId_ReturnsNotFound(
        string action)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsync(
            $"/organization-invitations/{Guid.Empty}/{action}",
            null);

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.NotFound,
            OrganizationInvitationErrorCodes.InvitationNotFound);
    }

    [Theory]
    [InlineData("{ invalid json")]
    [InlineData("{\"invitedUserId\":")]
    [InlineData("[]")]
    public async Task CreateInvitation_WithInvalidJson_ReturnsBadRequest(
        string json)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await scenario.OwnerClient.PostAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExistingInvitation_RespectsOrganizationAccess()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var creationResponse = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        Assert.Equal(HttpStatusCode.Created, creationResponse.StatusCode);

        using var creationDocument = JsonDocument.Parse(
            await creationResponse.Content.ReadAsStringAsync());

        var invitationId = creationDocument.RootElement
            .GetProperty("id")
            .GetGuid();

        using var recipientResponse = await scenario.InvitedClient.GetAsync(
            $"/organization-invitations/{invitationId}");

        Assert.Equal(HttpStatusCode.OK, recipientResponse.StatusCode);

        using var ownerResponse = await scenario.OwnerClient.GetAsync(
            $"/organization-invitations/{invitationId}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);

        using var organizationListResponse = await scenario.OwnerClient.GetAsync(
            $"/organizations/{scenario.OrganizationId}/invitations");

        Assert.Equal(HttpStatusCode.OK, organizationListResponse.StatusCode);

        using var organizationListDocument = JsonDocument.Parse(
            await organizationListResponse.Content.ReadAsStringAsync());

        Assert.Contains(
            organizationListDocument.RootElement.EnumerateArray(),
            invitation => invitation.GetProperty("id").GetGuid() == invitationId);

        const string thirdExternalUserId = "user_test_789";

        factory.ExternalIdentityUserId = thirdExternalUserId;
        factory.ExternalIdentityEmail = "third-user@example.com";

        using var externalClient = CreateSecondAuthenticatedClient(
            thirdExternalUserId);

        using var externalSession = await externalClient.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, externalSession.StatusCode);

        using var externalInvitationResponse = await externalClient.GetAsync(
            $"/organization-invitations/{invitationId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            externalInvitationResponse.StatusCode);

        using var externalListResponse = await externalClient.GetAsync(
            $"/organizations/{scenario.OrganizationId}/invitations");

        Assert.Equal(
            HttpStatusCode.NotFound,
            externalListResponse.StatusCode);

        using var personalListResponse = await externalClient.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(HttpStatusCode.OK, personalListResponse.StatusCode);

        using var personalListDocument = JsonDocument.Parse(
            await personalListResponse.Content.ReadAsStringAsync());

        Assert.Empty(personalListDocument.RootElement.EnumerateArray());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing-at-sign.example.com")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public async Task CreateInvitation_WithEmptyEmail_ReturnsBadRequest(
        string? email)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email,
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.BadRequest,
            OrganizationInvitationErrorCodes.InvalidRecipientEmail);
    }

    [Fact]
    public async Task CreateInvitation_WithUnknownEmail_ReturnsNotFound()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "missing-user@example.com",
                roleId = scenario.MemberRoleId,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.NotFound,
            OrganizationInvitationErrorCodes.RecipientUnavailable);
    }

    [Fact]
    public async Task CreateInvitation_WithEmptyRoleId_ReturnsBadRequest()
    {
        using var scenario = await CreateInvitationScenarioAsync();

        using var response = await scenario.OwnerClient.PostAsJsonAsync(
            $"/organizations/{scenario.OrganizationId}/invitations",
            new
            {
                email = "second-user@example.com",
                roleId = Guid.Empty,
                duration = 7
            });

        await AssertApplicationErrorAsync(
            response,
            HttpStatusCode.BadRequest,
            OrganizationInvitationErrorCodes.InvalidRoleId);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ExpiredInvitation_ReturnsEffectiveStatusAndRejectsTransition(
        string action)
    {
        using var scenario = await CreateInvitationScenarioAsync();

        var owner = await factory.Database.GetUserAsync(
            SignalLabApiFactory.TestExternalUserId);

        Assert.NotNull(owner);

        var now = DateTimeOffset.UtcNow;

        var createdAt = new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero).AddDays(-10);

        var invitation = new OrganizationInvitation(
            new OrganizationId(scenario.OrganizationId),
            new UserId(scenario.InvitedUserId),
            owner.Id,
            new RoleId(scenario.MemberRoleId),
            createdAt,
            createdAt.AddDays(7));

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.OrganizationInvitations.Add(invitation);

            await dbContext.SaveChangesAsync();
        }

        using var detailsResponse = await scenario.InvitedClient.GetAsync(
            $"/organization-invitations/{invitation.Id.Value}");

        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);

        using var detailsDocument = JsonDocument.Parse(
            await detailsResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            "Expired",
            detailsDocument.RootElement.GetProperty("status").GetString());

        using var personalResponse = await scenario.InvitedClient.GetAsync(
            "/users/me/organization-invitations");

        Assert.Equal(HttpStatusCode.OK, personalResponse.StatusCode);

        using var personalDocument = JsonDocument.Parse(
            await personalResponse.Content.ReadAsStringAsync());

        var personalInvitation = Assert.Single(
            personalDocument.RootElement.EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == invitation.Id.Value);

        Assert.Equal(
            "Expired",
            personalInvitation.GetProperty("status").GetString());

        var actingClient = action == "cancel"
            ? scenario.OwnerClient
            : scenario.InvitedClient;

        using var transitionResponse = await actingClient.PostAsync(
            $"/organization-invitations/{invitation.Id.Value}/{action}",
            null);

        await AssertApplicationErrorAsync(
            transitionResponse,
            HttpStatusCode.Conflict,
            OrganizationInvitationErrorCodes.InvalidState);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var storedInvitation = await dbContext.OrganizationInvitations
                .AsNoTracking()
                .SingleAsync(item => item.Id == invitation.Id);

            Assert.Equal(
                OrganizationInvitationStatus.Pending,
                storedInvitation.Status);

            Assert.Equal(createdAt, storedInvitation.UpdatedAt);

            Assert.Null(storedInvitation.RespondedAt);
        }

        using var organizationResponse = await scenario.InvitedClient.GetAsync(
            $"/organizations/{scenario.OrganizationId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            organizationResponse.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
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

    private HttpClient CreateSecondAuthenticatedClient(
        string externalUserId)
    {
        var client = CreateClient();

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            externalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static async Task AssertApplicationErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            expectedCode,
            document.RootElement.GetProperty("code").GetString());

        Assert.Equal(
            (int)expectedStatus,
            document.RootElement.GetProperty("status").GetInt32());
    }

    private async Task<InvitationScenario> CreateInvitationScenarioAsync()
    {
        var ownerClient = CreateAuthenticatedClient();
        HttpClient? invitedClient = null;

        try
        {
            using var ownerSession = await ownerClient.GetAsync(
                "/authentication/session");

            Assert.Equal(HttpStatusCode.OK, ownerSession.StatusCode);

            using var organizationResponse = await ownerClient.PostAsJsonAsync(
                "/organizations",
                new
                {
                    name = "SignalLab Studio",
                    country = "FR",
                    website = (string?)null
                });

            Assert.Equal(
                HttpStatusCode.Created,
                organizationResponse.StatusCode);

            using var organizationDocument = JsonDocument.Parse(
                await organizationResponse.Content.ReadAsStringAsync());

            var organizationId = organizationDocument.RootElement
                .GetProperty("id")
                .GetGuid();

            using var rolesResponse = await ownerClient.GetAsync(
                $"/organizations/{organizationId}/roles");

            Assert.Equal(HttpStatusCode.OK, rolesResponse.StatusCode);

            using var rolesDocument = JsonDocument.Parse(
                await rolesResponse.Content.ReadAsStringAsync());

            var memberRoleId = rolesDocument.RootElement
                .EnumerateArray()
                .Single(role =>
                    role.GetProperty("name").GetString() == "Member")
                .GetProperty("id")
                .GetGuid();

            const string secondExternalUserId = "user_test_456";

            factory.ExternalIdentityUserId = secondExternalUserId;
            factory.ExternalIdentityEmail = "second-user@example.com";

            invitedClient = CreateSecondAuthenticatedClient(
                secondExternalUserId);

            using var invitedSession = await invitedClient.GetAsync(
                "/authentication/session");

            Assert.Equal(HttpStatusCode.OK, invitedSession.StatusCode);

            var invitedUser = await factory.Database.GetUserAsync(
                secondExternalUserId);

            Assert.NotNull(invitedUser);

            return new InvitationScenario(
                ownerClient,
                invitedClient,
                organizationId,
                memberRoleId,
                invitedUser.Id.Value);
        }
        catch
        {
            invitedClient?.Dispose();
            ownerClient.Dispose();

            throw;
        }
    }

    private sealed record InvitationScenario(
        HttpClient OwnerClient,
        HttpClient InvitedClient,
        Guid OrganizationId,
        Guid MemberRoleId,
        Guid InvitedUserId) : IDisposable
    {
        public void Dispose()
        {
            InvitedClient.Dispose();
            OwnerClient.Dispose();
        }
    }
}