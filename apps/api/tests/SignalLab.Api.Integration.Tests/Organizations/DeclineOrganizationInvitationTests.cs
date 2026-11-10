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
public sealed class DeclineOrganizationInvitationTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public DeclineOrganizationInvitationTests(SignalLabApiFactory factory)
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
    public async Task DeclineInvitation_ShouldPersistStatusWithoutCreatingMembership()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var declined = await ExecuteAsync(invitation.Id, recipient.Id);

        Assert.Equal(OrganizationInvitationStatus.Declined, declined.Status);
        Assert.NotNull(declined.RespondedAt);
        Assert.Equal(declined.RespondedAt, declined.UpdatedAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(OrganizationInvitationStatus.Declined, stored.Status);
        Assert.NotNull(stored.RespondedAt);
        Assert.Equal(stored.RespondedAt, stored.UpdatedAt);

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(member =>
                member.OrganizationId == organization.Id &&
                member.UserId == recipient.Id));
    }

    [Fact]
    public async Task OtherUser_ShouldNotDeclineInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(invitation.Id, owner.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvitationNotFound,
            exception.Code);

        await AssertPendingAsync(invitation.Id);
    }

    [Fact]
    public async Task UnknownInvitation_ShouldBeRejected()
    {
        var recipient = await CreateUserAsync();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(
                new OrganizationInvitationId(Guid.NewGuid()),
                recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvitationNotFound,
            exception.Code);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldNotBeDeclined()
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

        await AssertPendingAsync(invitation.Id);
    }

    [Theory]
    [InlineData(OrganizationInvitationStatus.Accepted)]
    [InlineData(OrganizationInvitationStatus.Cancelled)]
    public async Task CompletedInvitation_ShouldNotBeDeclined(
        OrganizationInvitationStatus status)
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var createdAt = UtcNow().AddHours(-1);

        var invitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            GetMemberRole(organization).Id,
            createdAt,
            createdAt.AddDays(7));

        var responseAt = createdAt.AddMinutes(5);

        if (status == OrganizationInvitationStatus.Accepted)
        {
            invitation.Accept(recipient.Id, responseAt);
        }
        else
        {
            invitation.Cancel(responseAt);
        }

        await PersistInvitationAsync(invitation);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(status, stored.Status);
        Assert.Equal(invitation.RespondedAt, stored.RespondedAt);
    }

    [Fact]
    public async Task DeclinedInvitation_ShouldNotBeDeclinedTwice()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var first = await ExecuteAsync(invitation.Id, recipient.Id);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(OrganizationInvitationStatus.Declined, stored.Status);
        Assert.Equal(first.RespondedAt, stored.RespondedAt);
        Assert.Equal(first.UpdatedAt, stored.UpdatedAt);
    }

    [Fact]
    public async Task DeclineInvitation_ShouldPreserveExistingNotification()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var notification = new Notification(
            recipient.Id,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                invitation.Id.Value),
            UtcNow());

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.Notifications.Add(notification);

            await dbContext.SaveChangesAsync();
        }

        await ExecuteAsync(invitation.Id, recipient.Id);

        using var verificationScope = factory.Services.CreateScope();

        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await verificationContext.Notifications
            .AsNoTracking()
            .SingleAsync(item => item.Id == notification.Id);

        Assert.Equal(recipient.Id, stored.UserId);
        Assert.Equal(invitation.Id.Value, stored.Target.Id);
        Assert.Null(stored.ReadAt);
    }

    [Fact]
    public async Task ConcurrentDeclines_ShouldAllowOnlyOneSuccess()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<Exception?> TryDeclineAsync()
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

        var firstTask = TryDeclineAsync();
        var secondTask = TryDeclineAsync();

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

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(OrganizationInvitationStatus.Declined, stored.Status);
        Assert.NotNull(stored.RespondedAt);

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(member =>
                member.OrganizationId == organization.Id &&
                member.UserId == recipient.Id));
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = new Organization(
            owner.Id,
            "Invitation Decline Organization",
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
            $"invitation-decline-{Guid.NewGuid():N}",
            $"invitation-decline-{Guid.NewGuid():N}@example.com",
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

    private async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<DeclineOrganizationInvitation>();

        return await useCase.ExecuteAsync(
            invitationId,
            currentUserId,
            CancellationToken.None);
    }

    private async Task AssertPendingAsync(
        OrganizationInvitationId invitationId)
    {
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitationId);

        Assert.Equal(OrganizationInvitationStatus.Pending, stored.Status);
        Assert.Null(stored.RespondedAt);
        Assert.Equal(stored.CreatedAt, stored.UpdatedAt);
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