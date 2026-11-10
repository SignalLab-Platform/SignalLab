using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class GetOrganizationInvitationsTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public GetOrganizationInvitationsTests(SignalLabApiFactory factory)
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
    public async Task OrganizationWithoutInvitations_ShouldReturnEmptyList()
    {
        var (organization, owner, _) = await CreateScenarioAsync();

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);
        Assert.Empty(invitations);
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("Member")]
    public async Task ActiveMember_ShouldBeAllowedToViewInvitations(
        string roleName)
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var member = await CreateUserAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            member.Id,
            roleName);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(
            organization.Id,
            member.Id);

        Assert.NotNull(invitations);
        Assert.Single(invitations);
        Assert.Equal(invitation.Id, invitations[0].Id);
    }

    [Fact]
    public async Task NonMember_ShouldNotAccessOrganizationInvitations()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var outsider = await CreateUserAsync();

        await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(
            organization.Id,
            outsider.Id);

        Assert.Null(invitations);
    }

    [Fact]
    public async Task RemovedMember_ShouldNotAccessOrganizationInvitations()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var removedMember = await CreateUserAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            removedMember.Id,
            "Member");

        await factory.Database.RemoveOrganizationMemberAsync(
            organization.Id,
            removedMember.Id);

        await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(
            organization.Id,
            removedMember.Id);

        Assert.Null(invitations);
    }

    [Fact]
    public async Task InvitationsFromAnotherOrganization_ShouldNotBeReturned()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var otherOrganization = await CreateOrganizationAsync(
            owner,
            "Other Invitation Organization");

        var expectedInvitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        await CreateInvitationAsync(
            otherOrganization,
            owner,
            recipient);

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);
        Assert.Single(invitations);
        Assert.Equal(expectedInvitation.Id, invitations[0].Id);
        Assert.Equal(organization.Id, invitations[0].OrganizationId);
    }

    [Fact]
    public async Task Invitations_ShouldBeOrderedByCreationDateDescending()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var secondRecipient = await CreateUserAsync();
        var thirdRecipient = await CreateUserAsync();

        var now = UtcNow();

        var oldest = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            now.AddDays(-3));

        var middle = await CreateInvitationAsync(
            organization,
            owner,
            secondRecipient,
            now.AddDays(-2));

        var newest = await CreateInvitationAsync(
            organization,
            owner,
            thirdRecipient,
            now.AddDays(-1));

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);
        Assert.Equal(3, invitations.Count);

        Assert.Equal(newest.Id, invitations[0].Id);
        Assert.Equal(middle.Id, invitations[1].Id);
        Assert.Equal(oldest.Id, invitations[2].Id);
    }

    [Fact]
    public async Task Invitations_ShouldReturnRecipientRoleAndDates()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddHours(-1);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            createdAt);

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);

        var details = Assert.Single(invitations);
        var role = GetRole(organization, "Member");

        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(organization.Id, details.OrganizationId);

        Assert.Equal(recipient.Id, details.InvitedUserId);
        Assert.Equal(recipient.FirstName, details.InvitedUserFirstName);
        Assert.Equal(recipient.LastName, details.InvitedUserLastName);
        Assert.Equal(recipient.Email, details.InvitedUserEmail);

        Assert.Equal(owner.Id, details.InvitedByUserId);

        Assert.Equal(role.Id, details.RoleId);
        Assert.Equal(role.Name, details.RoleName);

        Assert.Equal(OrganizationInvitationStatus.Pending, details.Status);

        Assert.Equal(invitation.CreatedAt, details.CreatedAt);
        Assert.Equal(invitation.UpdatedAt, details.UpdatedAt);
        Assert.Equal(invitation.ExpiresAt, details.ExpiresAt);
        Assert.Null(details.RespondedAt);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldReturnEffectiveStatusWithoutPersisting()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddDays(-10);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            createdAt);

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);

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

        Assert.Equal(
            invitation.CreatedAt,
            storedInvitation.UpdatedAt);
    }

    [Fact]
    public async Task CompletedInvitation_ShouldRemainVisibleInHistory()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddDays(-2);

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetRole(organization, "Member").Id,
            createdAt,
            createdAt.AddDays(7));

        var respondedAt = createdAt.AddHours(1);

        invitation.Decline(recipient.Id, respondedAt);

        await PersistInvitationAsync(invitation);

        var invitations = await ExecuteAsync(
            organization.Id,
            owner.Id);

        Assert.NotNull(invitations);

        var details = Assert.Single(invitations);

        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(OrganizationInvitationStatus.Declined, details.Status);
        Assert.Equal(respondedAt, details.RespondedAt);
        Assert.Equal(respondedAt, details.UpdatedAt);
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = await CreateOrganizationAsync(
            owner,
            "Invitation Consultation Organization");

        return (organization, owner, recipient);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = new User(
            $"invitation-consultation-{Guid.NewGuid():N}",
            $"invitation-consultation-{Guid.NewGuid():N}@example.com",
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
            GetRole(organization, "Member").Id,
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

    private async Task<IReadOnlyList<OrganizationInvitationDetails>?> ExecuteAsync(
        OrganizationId organizationId,
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<GetOrganizationInvitations>();

        return await useCase.ExecuteAsync(
            organizationId,
            currentUserId,
            CancellationToken.None);
    }

    private static Role GetRole(
        Organization organization,
        string roleName)
    {
        return organization.Roles.Single(
            role => role.Name == roleName);
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}