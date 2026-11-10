using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Errors;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class AcceptOrganizationInvitationTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public AcceptOrganizationInvitationTests(SignalLabApiFactory factory)
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
    public async Task AcceptInvitation_ShouldCreateMembershipAndPersistAcceptance()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var accepted = await ExecuteAsync(
            invitation.Id,
            recipient.Id);

        Assert.Equal(OrganizationInvitationStatus.Accepted, accepted.Status);
        Assert.NotNull(accepted.RespondedAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var storedInvitation = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        var member = await dbContext.OrganizationMembers
            .AsNoTracking()
            .SingleAsync(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id);

        Assert.Equal(
            OrganizationInvitationStatus.Accepted,
            storedInvitation.Status);

        Assert.NotNull(storedInvitation.RespondedAt);
        Assert.Equal(storedInvitation.RespondedAt, storedInvitation.UpdatedAt);

        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(invitation.RoleId, member.RoleId);
        Assert.Null(member.RemovedAt);
    }

    [Fact]
    public async Task RemovedMember_ShouldBeReactivatedWithSameIdAndNewRole()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            recipient.Id,
            "Member");

        OrganizationMemberId originalMemberId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            originalMemberId = await dbContext.OrganizationMembers
                .Where(item =>
                    item.OrganizationId == organization.Id &&
                    item.UserId == recipient.Id)
                .Select(item => item.Id)
                .SingleAsync();
        }

        await factory.Database.RemoveOrganizationMemberAsync(
            organization.Id,
            recipient.Id);

        var administratorRole = GetRole(organization, "Administrator");

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            roleId: administratorRole.Id);

        await ExecuteAsync(invitation.Id, recipient.Id);

        using var verificationScope = factory.Services.CreateScope();

        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var memberships = await verificationContext.OrganizationMembers
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id)
            .ToListAsync();

        var member = Assert.Single(memberships);

        Assert.Equal(originalMemberId, member.Id);
        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(administratorRole.Id, member.RoleId);
        Assert.Null(member.RemovedAt);
    }

    [Fact]
    public async Task OtherUser_ShouldNotAcceptInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var outsider = await CreateUserAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(invitation.Id, outsider.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvitationNotFound,
            exception.Code);

        await AssertPendingWithoutMembershipAsync(
            invitation.Id,
            organization.Id,
            recipient.Id);
    }

    [Fact]
    public async Task UnknownInvitation_ShouldBeRejected()
    {
        var user = await CreateUserAsync();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(
                new OrganizationInvitationId(Guid.NewGuid()),
                user.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvitationNotFound,
            exception.Code);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldNotBeAccepted()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            UtcNow().AddDays(-10));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        await AssertPendingWithoutMembershipAsync(
            invitation.Id,
            organization.Id,
            recipient.Id);
    }

    [Fact]
    public async Task DeclinedInvitation_ShouldNotBeAccepted()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddHours(-1);

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetRole(organization, "Member").Id,
            createdAt,
            createdAt.AddDays(7));

        invitation.Decline(recipient.Id, createdAt.AddMinutes(1));

        await PersistInvitationAsync(invitation);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id));
    }

    [Fact]
    public async Task AcceptedInvitation_ShouldNotBeAcceptedTwice()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        await ExecuteAsync(invitation.Id, recipient.Id);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var membershipCount = await dbContext.OrganizationMembers
            .CountAsync(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id);

        Assert.Equal(1, membershipCount);
    }

    [Fact]
    public async Task ExistingActiveMember_ShouldNotAcceptInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            recipient.Id,
            "Member");

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.AlreadyMember,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var storedInvitation = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            storedInvitation.Status);

        Assert.Null(storedInvitation.RespondedAt);

        Assert.Equal(1, await dbContext.OrganizationMembers
            .CountAsync(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id));
    }

    [Fact]
    public async Task FailedSave_ShouldRollbackInvitationAndMembership()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var useCase = scope.ServiceProvider
                .GetRequiredService<AcceptOrganizationInvitation>();

            var invalidNotification = new Notification(
                new UserId(Guid.NewGuid()),
                NotificationType.OrganizationInvitationReceived,
                new NotificationTarget(
                    NotificationTargetType.OrganizationInvitation,
                    invitation.Id.Value),
                UtcNow());

            dbContext.Notifications.Add(invalidNotification);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => useCase.ExecuteAsync(
                    invitation.Id,
                    recipient.Id,
                    CancellationToken.None));
        }

        await AssertPendingWithoutMembershipAsync(
            invitation.Id,
            organization.Id,
            recipient.Id);
    }

    [Fact]
    public async Task ConcurrentAcceptance_ShouldCreateOnlyOneMembership()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<Exception?> TryAcceptAsync()
        {
            await start.Task;

            try
            {
                await ExecuteAsync(invitation.Id, recipient.Id);
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        var firstTask = TryAcceptAsync();
        var secondTask = TryAcceptAsync();

        start.SetResult();

        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.Single(results, result => result is null);

        Assert.Single(
            results,
            exception =>
                exception is ConflictException conflict &&
                conflict.Code == OrganizationInvitationErrorCodes.InvalidState);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var memberships = await dbContext.OrganizationMembers
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id)
            .ToListAsync();

        var member = Assert.Single(memberships);

        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(invitation.RoleId, member.RoleId);

        var storedInvitation = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(
            OrganizationInvitationStatus.Accepted,
            storedInvitation.Status);
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = new Organization(
            owner.Id,
            "Invitation Acceptance Organization",
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
            $"invitation-acceptance-{Guid.NewGuid():N}",
            $"invitation-acceptance-{Guid.NewGuid():N}@example.com",
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
        DateTimeOffset? createdAt = null,
        RoleId? roleId = null)
    {
        var now = createdAt ?? UtcNow();

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            roleId ?? GetRole(organization, "Member").Id,
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

    private async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<AcceptOrganizationInvitation>();

        return await useCase.ExecuteAsync(
            invitationId,
            currentUserId,
            CancellationToken.None);
    }

    private async Task AssertPendingWithoutMembershipAsync(
        OrganizationInvitationId invitationId,
        OrganizationId organizationId,
        UserId recipientId)
    {
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var invitation = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitationId);

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitation.Status);

        Assert.Null(invitation.RespondedAt);

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(item =>
                item.OrganizationId == organizationId &&
                item.UserId == recipientId));
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