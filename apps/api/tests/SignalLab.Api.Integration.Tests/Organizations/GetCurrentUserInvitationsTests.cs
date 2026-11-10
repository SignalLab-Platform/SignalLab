using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class GetCurrentUserInvitationsTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public GetCurrentUserInvitationsTests(SignalLabApiFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await factory.Database.ResetOrganizationsAsync(createdOrganizationIds);
        await factory.Database.ResetUsersAsync(createdUserIds);
    }

    [Fact]
    public async Task UserWithoutInvitations_ShouldReturnEmptyList()
    {
        var user = await CreateUserAsync();

        var invitations = await ExecuteAsync(user.Id);

        Assert.Empty(invitations);
    }

    [Fact]
    public async Task RecipientWithoutMembership_ShouldSeeInvitation()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(recipient.Id);

        var details = Assert.Single(invitations);

        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(organization.Id, details.OrganizationId);
        Assert.Equal(organization.Name, details.OrganizationName);
    }

    [Fact]
    public async Task User_ShouldOnlySeeOwnInvitations()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();
        var otherRecipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var expectedInvitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        await CreateInvitationAsync(
            organization,
            owner,
            otherRecipient);

        var invitations = await ExecuteAsync(recipient.Id);

        var details = Assert.Single(invitations);

        Assert.Equal(expectedInvitation.Id, details.Id);
        Assert.Equal(recipient.Id, expectedInvitation.InvitedUserId);
    }

    [Fact]
    public async Task InvitationsFromMultipleOrganizations_ShouldBeResolved()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var firstOrganization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var secondOrganization = await CreateOrganizationAsync(
            owner,
            "Organization B");

        var firstInvitation = await CreateInvitationAsync(
            firstOrganization,
            owner,
            recipient);

        var secondInvitation = await CreateInvitationAsync(
            secondOrganization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(recipient.Id);

        Assert.Equal(2, invitations.Count);

        Assert.Contains(invitations, details =>
            details.Id == firstInvitation.Id &&
            details.OrganizationName == firstOrganization.Name);

        Assert.Contains(invitations, details =>
            details.Id == secondInvitation.Id &&
            details.OrganizationName == secondOrganization.Name);
    }

    [Fact]
    public async Task Invitation_ShouldReturnRoleAuthorAndDates()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var createdAt = UtcNow().AddHours(-1);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            createdAt);

        var invitations = await ExecuteAsync(recipient.Id);

        var details = Assert.Single(invitations);

        var role = organization.Roles.Single(
            role => role.Name == "Member");

        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(owner.Id, details.InvitedByUserId);

        Assert.Equal(role.Id, details.RoleId);
        Assert.Equal(role.Name, details.RoleName);

        Assert.Equal(OrganizationInvitationStatus.Pending, details.Status);

        Assert.Equal(createdAt, details.CreatedAt);
        Assert.Equal(createdAt, details.UpdatedAt);
        Assert.Equal(createdAt.AddDays(7), details.ExpiresAt);
        Assert.Null(details.RespondedAt);
    }

    [Fact]
    public async Task Invitations_ShouldBeOrderedByCreationDateDescending()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var firstOrganization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var secondOrganization = await CreateOrganizationAsync(
            owner,
            "Organization B");

        var now = UtcNow();

        var oldest = await CreateInvitationAsync(
            firstOrganization,
            owner,
            recipient,
            now.AddDays(-3));

        var newest = await CreateInvitationAsync(
            secondOrganization,
            owner,
            recipient,
            now.AddDays(-1));

        var invitations = await ExecuteAsync(recipient.Id);

        Assert.Equal(2, invitations.Count);
        Assert.Equal(newest.Id, invitations[0].Id);
        Assert.Equal(oldest.Id, invitations[1].Id);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldReturnEffectiveStatusWithoutPersisting()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            UtcNow().AddDays(-10));

        var invitations = await ExecuteAsync(recipient.Id);

        var details = Assert.Single(invitations);

        Assert.Equal(
            OrganizationInvitationStatus.Expired,
            details.Status);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var storedInvitation = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            storedInvitation.Status);
    }

    [Fact]
    public async Task DeclinedInvitation_ShouldRemainVisibleInHistory()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Organization A");

        var createdAt = UtcNow().AddDays(-2);

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetMemberRole(organization).Id,
            createdAt,
            createdAt.AddDays(7));

        var respondedAt = createdAt.AddHours(1);

        invitation.Decline(recipient.Id, respondedAt);

        await PersistInvitationAsync(invitation);

        var invitations = await ExecuteAsync(recipient.Id);

        var details = Assert.Single(invitations);

        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(OrganizationInvitationStatus.Declined, details.Status);
        Assert.Equal(respondedAt, details.RespondedAt);
        Assert.Equal(respondedAt, details.UpdatedAt);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = new User(
            $"received-invitation-{Guid.NewGuid():N}",
            $"received-invitation-{Guid.NewGuid():N}@example.com",
            UtcNow());

        createdUserIds.Add(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<Organization> CreateOrganizationAsync(
        User owner,
        string name)
    {
        var organization = new Organization(
            owner.Id,
            name,
            "FR",
            null,
            UtcNow());

        createdOrganizationIds.Add(organization.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Organizations.Add(organization);

        await dbContext.SaveChangesAsync();

        return organization;
    }

    private async Task<OrganizationInvitation> CreateInvitationAsync(
        Organization organization,
        User owner,
        User recipient,
        DateTimeOffset? createdAt = null)
    {
        var now = createdAt ?? UtcNow();

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetMemberRole(organization).Id,
            now,
            now.AddDays(7));

        await PersistInvitationAsync(invitation);

        return invitation;
    }

    private async Task PersistInvitationAsync(
        OrganizationInvitation invitation)
    {
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.OrganizationInvitations.Add(invitation);

        await dbContext.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<CurrentUserInvitationDetails>> ExecuteAsync(
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<GetCurrentUserInvitations>();

        return await useCase.ExecuteAsync(
            currentUserId,
            CancellationToken.None);
    }

    private static Role GetMemberRole(Organization organization)
    {
        return organization.Roles.Single(
            role => role.Name == "Member");
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}