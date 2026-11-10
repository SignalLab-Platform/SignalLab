using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class GetOrganizationInvitationTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public GetOrganizationInvitationTests(SignalLabApiFactory factory)
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
    public async Task RecipientWithoutMembership_ShouldAccessInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var details = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.NotNull(details);
        Assert.Equal(invitation.Id, details.Id);
        Assert.Equal(organization.Id, details.OrganizationId);
        Assert.Equal(recipient.Id, details.InvitedUserId);
    }

    [Fact]
    public async Task Owner_ShouldAccessOrganizationInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var details = await ExecuteAsync(
            invitation.Id,
            owner.Id);

        Assert.NotNull(details);
        Assert.Equal(invitation.Id, details.Id);
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("Member")]
    public async Task ActiveMember_ShouldAccessOrganizationInvitation(
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

        var details = await ExecuteAsync(
            invitation.Id,
            member.Id);

        Assert.NotNull(details);
        Assert.Equal(invitation.Id, details.Id);
    }

    [Fact]
    public async Task NonMember_ShouldNotAccessOrganizationInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var outsider = await CreateUserAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var details = await ExecuteAsync(
            invitation.Id,
            outsider.Id);

        Assert.Null(details);
    }

    [Fact]
    public async Task RemovedMember_ShouldNotAccessOrganizationInvitation()
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

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var details = await ExecuteAsync(
            invitation.Id,
            removedMember.Id);

        Assert.Null(details);
    }

    [Fact]
    public async Task RemovedRecipient_ShouldStillAccessOwnInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            recipient.Id,
            "Member");

        await factory.Database.RemoveOrganizationMemberAsync(
            organization.Id,
            recipient.Id);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var details = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.NotNull(details);
        Assert.Equal(invitation.Id, details.Id);
    }

    [Fact]
    public async Task UnknownInvitation_ShouldReturnNull()
    {
        var user = await CreateUserAsync();

        var details = await ExecuteAsync(
            new OrganizationInvitationId(Guid.NewGuid()),
            user.Id);

        Assert.Null(details);
    }

    [Fact]
    public async Task Invitation_ShouldReturnRecipientRoleAndDates()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddHours(-1);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            createdAt);

        var details = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.NotNull(details);

        var role = GetMemberRole(organization);

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
        Assert.Equal(createdAt, details.CreatedAt);
        Assert.Equal(createdAt, details.UpdatedAt);
        Assert.Equal(createdAt.AddDays(7), details.ExpiresAt);
        Assert.Null(details.RespondedAt);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldReturnEffectiveStatusWithoutPersisting()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            UtcNow().AddDays(-10));

        var details = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.NotNull(details);

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
    public async Task DeclinedInvitation_ShouldRemainAccessible()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddDays(-2);

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetMemberRole(organization).Id,
            createdAt,
            createdAt.AddDays(7));

        var respondedAt = createdAt.AddMinutes(15);

        invitation.Decline(recipient.Id, respondedAt);

        await PersistInvitationAsync(invitation);

        var details = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.NotNull(details);
        Assert.Equal(OrganizationInvitationStatus.Declined, details.Status);
        Assert.Equal(respondedAt, details.RespondedAt);
        Assert.Equal(respondedAt, details.UpdatedAt);
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = new Organization(
            owner.Id,
            "Invitation Details Organization",
            "FR",
            null,
            UtcNow());

        createdOrganizationIds.Add(organization.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Organizations.Add(organization);

        await dbContext.SaveChangesAsync();

        return (organization, owner, recipient);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = new User(
            $"invitation-details-{Guid.NewGuid():N}",
            $"invitation-details-{Guid.NewGuid():N}@example.com",
            UtcNow());

        createdUserIds.Add(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
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

    private async Task<OrganizationInvitationDetails?> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<GetOrganizationInvitation>();

        return await useCase.ExecuteAsync(
            invitationId,
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