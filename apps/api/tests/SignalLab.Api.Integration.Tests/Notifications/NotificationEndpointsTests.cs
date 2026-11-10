using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SignalLab.Application.Notifications;

namespace SignalLab.Api.Integration.Tests.Notifications;

[Collection(SignalLabApiCollection.Name)]
public sealed class NotificationEndpointsTests :
    IClassFixture<SignalLabApiFactory>,
    IAsyncLifetime
{
    private const string SecondExternalUserId = "user_test_456";

    private readonly SignalLabApiFactory factory;

    public NotificationEndpointsTests(SignalLabApiFactory factory)
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
    [InlineData("GET", "/users/me/notifications")]
    [InlineData("GET", "/users/me/notifications/unread-count")]
    [InlineData("POST", "/users/me/notifications/00000000-0000-0000-0000-000000000001/read")]
    public async Task NotificationEndpoints_WithoutToken_ReturnUnauthorized(
        string method,
        string path)
    {
        using var client = CreateClient();

        using var request = new HttpRequestMessage(
            new HttpMethod(method),
            path);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_WithoutSession_ReturnsConflict()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync(
            "/users/me/notifications");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            "session_initialization_required",
            document.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetNotifications_WithoutNotifications_ReturnsEmptyListAndZeroCount()
    {
        using var client = CreateAuthenticatedClient();

        await InitializeSessionAsync(client);

        using var notificationsResponse = await client.GetAsync(
            "/users/me/notifications");

        Assert.Equal(
            HttpStatusCode.OK,
            notificationsResponse.StatusCode);

        using var notificationsDocument = JsonDocument.Parse(
            await notificationsResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            JsonValueKind.Array,
            notificationsDocument.RootElement.ValueKind);

        Assert.Equal(
            0,
            notificationsDocument.RootElement.GetArrayLength());

        using var countResponse = await client.GetAsync(
            "/users/me/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, countResponse.StatusCode);

        using var countDocument = JsonDocument.Parse(
            await countResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            0,
            countDocument.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task InvitationNotification_CanBeRead_AndUnreadCountDecreases()
    {
        using var ownerClient = CreateAuthenticatedClient();

        await InitializeSessionAsync(ownerClient);

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

        var roleId = rolesDocument.RootElement
            .EnumerateArray()
            .Single(role =>
                role.GetProperty("name").GetString() == "Member")
            .GetProperty("id")
            .GetGuid();

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var invitedClient = CreateAuthenticatedClient(
            SecondExternalUserId);

        await InitializeSessionAsync(invitedClient);

        var invitedUser = await factory.Database.GetUserAsync(
            SecondExternalUserId);

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

        using var notificationsResponse = await invitedClient.GetAsync(
            "/users/me/notifications");

        Assert.Equal(
            HttpStatusCode.OK,
            notificationsResponse.StatusCode);

        using var notificationsDocument = JsonDocument.Parse(
            await notificationsResponse.Content.ReadAsStringAsync());

        var notifications = notificationsDocument.RootElement;

        Assert.Equal(1, notifications.GetArrayLength());

        var notification = notifications[0];

        var notificationId = notification.GetProperty("id").GetGuid();

        Assert.Equal(
            "OrganizationInvitationReceived",
            notification.GetProperty("type").GetString());

        Assert.Equal(
            "OrganizationInvitation",
            notification.GetProperty("targetType").GetString());

        Assert.Equal(
            invitationId,
            notification.GetProperty("targetId").GetGuid());

        Assert.Equal(
            JsonValueKind.Null,
            notification.GetProperty("readAt").ValueKind);

        using var unreadBefore = await invitedClient.GetAsync(
            "/users/me/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, unreadBefore.StatusCode);

        using var unreadBeforeDocument = JsonDocument.Parse(
            await unreadBefore.Content.ReadAsStringAsync());

        Assert.Equal(
            1,
            unreadBeforeDocument.RootElement.GetProperty("count").GetInt32());

        using var readResponse = await invitedClient.PostAsync(
            $"/users/me/notifications/{notificationId}/read",
            null);

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        using var readDocument = JsonDocument.Parse(
            await readResponse.Content.ReadAsStringAsync());

        var readAt = readDocument.RootElement
            .GetProperty("readAt")
            .GetDateTimeOffset();

        using var unreadAfter = await invitedClient.GetAsync(
            "/users/me/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, unreadAfter.StatusCode);

        using var unreadAfterDocument = JsonDocument.Parse(
            await unreadAfter.Content.ReadAsStringAsync());

        Assert.Equal(
            0,
            unreadAfterDocument.RootElement.GetProperty("count").GetInt32());

        using var secondReadResponse = await invitedClient.PostAsync(
            $"/users/me/notifications/{notificationId}/read",
            null);

        Assert.Equal(HttpStatusCode.OK, secondReadResponse.StatusCode);

        using var secondReadDocument = JsonDocument.Parse(
            await secondReadResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            readAt,
            secondReadDocument.RootElement
                .GetProperty("readAt")
                .GetDateTimeOffset());

        using var ownerNotificationsResponse = await ownerClient.GetAsync(
            "/users/me/notifications");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerNotificationsResponse.StatusCode);

        using var ownerNotificationsDocument = JsonDocument.Parse(
            await ownerNotificationsResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            0,
            ownerNotificationsDocument.RootElement.GetArrayLength());

        using var ownerReadResponse = await ownerClient.PostAsync(
            $"/users/me/notifications/{notificationId}/read",
            null);

        await AssertNotificationNotFoundAsync(ownerReadResponse);

        using var unknownReadResponse = await invitedClient.PostAsync(
            $"/users/me/notifications/{Guid.NewGuid()}/read",
            null);

        await AssertNotificationNotFoundAsync(unknownReadResponse);
    }

    [Fact]
    public async Task GetNotifications_WithMoreThanTwenty_ReturnsTwentyButCountsAllUnread()
    {
        using var ownerClient = CreateAuthenticatedClient();

        await InitializeSessionAsync(ownerClient);

        factory.ExternalIdentityUserId = SecondExternalUserId;
        factory.ExternalIdentityEmail = "second-user@example.com";

        using var invitedClient = CreateAuthenticatedClient(
            SecondExternalUserId);

        await InitializeSessionAsync(invitedClient);

        var invitedUser = await factory.Database.GetUserAsync(
            SecondExternalUserId);

        Assert.NotNull(invitedUser);

        const int invitationCount = 21;

        var createdInvitationIds = new List<Guid>();

        for (var index = 0; index < invitationCount; index++)
        {
            using var organizationResponse = await ownerClient.PostAsJsonAsync(
                "/organizations",
                new
                {
                    name = $"SignalLab Studio {index}",
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

            var roleId = rolesDocument.RootElement
                .EnumerateArray()
                .Single(role =>
                    role.GetProperty("name").GetString() == "Member")
                .GetProperty("id")
                .GetGuid();

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

            createdInvitationIds.Add(
                invitationDocument.RootElement.GetProperty("id").GetGuid());
        }

        using var notificationsResponse = await invitedClient.GetAsync(
            "/users/me/notifications");

        Assert.Equal(HttpStatusCode.OK, notificationsResponse.StatusCode);

        using var notificationsDocument = JsonDocument.Parse(
            await notificationsResponse.Content.ReadAsStringAsync());

        var notifications = notificationsDocument.RootElement;

        Assert.Equal(20, notifications.GetArrayLength());

        var distinctNotificationIds = notifications
            .EnumerateArray()
            .Select(notification => notification.GetProperty("id").GetGuid())
            .Distinct()
            .Count();

        Assert.Equal(20, distinctNotificationIds);

        var returnedInvitationIds = notifications
            .EnumerateArray()
            .Select(notification => notification.GetProperty("targetId").GetGuid())
            .ToArray();

        var expectedInvitationIds = createdInvitationIds
            .Skip(1)
            .Reverse()
            .ToArray();

        Assert.Equal(
            expectedInvitationIds,
            returnedInvitationIds);

        Assert.All(
            notifications.EnumerateArray(),
            notification =>
            {
                Assert.Equal(
                    JsonValueKind.Null,
                    notification.GetProperty("readAt").ValueKind);
            });

        using var countResponse = await invitedClient.GetAsync(
            "/users/me/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, countResponse.StatusCode);

        using var countDocument = JsonDocument.Parse(
            await countResponse.Content.ReadAsStringAsync());

        Assert.Equal(
            invitationCount,
            countDocument.RootElement.GetProperty("count").GetInt32());
    }

    private static async Task AssertNotificationNotFoundAsync(
        HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            NotificationErrorCodes.NotificationNotFound,
            document.RootElement.GetProperty("code").GetString());

        Assert.Equal(
            404,
            document.RootElement.GetProperty("status").GetInt32());
    }

    private static async Task InitializeSessionAsync(HttpClient client)
    {
        using var response = await client.GetAsync(
            "/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private HttpClient CreateAuthenticatedClient(
        string externalUserId = SignalLabApiFactory.TestExternalUserId)
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
}