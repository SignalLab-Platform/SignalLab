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
public sealed class CancelOrganizationInvitationTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public CancelOrganizationInvitationTests(SignalLabApiFactory factory)
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
    public async Task Owner_ShouldCancelInvitationWithoutCreatingMembership()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var cancelled = await ExecuteAsync(invitation.Id, owner.Id);

        Assert.Equal(OrganizationInvitationStatus.Cancelled, cancelled.Status);
        Assert.Null(cancelled.RespondedAt);
        Assert.True(cancelled.UpdatedAt >= cancelled.CreatedAt);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(OrganizationInvitationStatus.Cancelled, stored.Status);
        Assert.Equal(cancelled.UpdatedAt, stored.UpdatedAt);
        Assert.Null(stored.RespondedAt);

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(member =>
                member.OrganizationId == organization.Id &&
                member.UserId == recipient.Id));
    }

    [Fact]
    public async Task Administrator_ShouldBeAllowedToCancelInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var administrator = await CreateUserAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            administrator.Id,
            "Administrator");

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var cancelled = await ExecuteAsync(
            invitation.Id,
            administrator.Id);

        Assert.Equal(OrganizationInvitationStatus.Cancelled, cancelled.Status);
    }

    [Fact]
    public async Task MemberWithoutManageMembers_ShouldNotCancelInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var member = await CreateUserAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            member.Id,
            "Member");

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var exception = await Assert.ThrowsAsync<PermissionDeniedException>(
            () => ExecuteAsync(invitation.Id, member.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.ManageMembersDenied,
            exception.Code);

        await AssertPendingAsync(invitation.Id);
    }

    [Fact]
    public async Task RemovedMember_ShouldNotCancelInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var removedMember = await CreateUserAsync();

        await factory.Database.AddOrganizationMemberAsync(
            organization.Id,
            removedMember.Id,
            "Administrator");

        await factory.Database.RemoveOrganizationMemberAsync(
            organization.Id,
            removedMember.Id);

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(invitation.Id, removedMember.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.OrganizationNotFound,
            exception.Code);

        await AssertPendingAsync(invitation.Id);
    }

    [Fact]
    public async Task RecipientWithoutPermission_ShouldNotCancelInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(invitation.Id, recipient.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.OrganizationNotFound,
            exception.Code);

        await AssertPendingAsync(invitation.Id);
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
    public async Task ExpiredInvitation_ShouldNotBeCancelled()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient,
            UtcNow().AddDays(-10));

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, owner.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        await AssertPendingAsync(invitation.Id);
    }

    [Theory]
    [InlineData(OrganizationInvitationStatus.Accepted)]
    [InlineData(OrganizationInvitationStatus.Declined)]
    public async Task CompletedInvitation_ShouldNotBeCancelled(
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

        var respondedAt = createdAt.AddMinutes(5);

        if (status == OrganizationInvitationStatus.Accepted)
        {
            invitation.Accept(recipient.Id, respondedAt);
        }
        else
        {
            invitation.Decline(recipient.Id, respondedAt);
        }

        await PersistInvitationAsync(invitation);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, owner.Id));

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
        Assert.Equal(respondedAt, stored.RespondedAt);
    }

    [Fact]
    public async Task CancelledInvitation_ShouldNotBeCancelledTwice()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var first = await ExecuteAsync(invitation.Id, owner.Id);

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(invitation.Id, owner.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidState,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var stored = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .SingleAsync(item => item.Id == invitation.Id);

        Assert.Equal(OrganizationInvitationStatus.Cancelled, stored.Status);
        Assert.Equal(first.UpdatedAt, stored.UpdatedAt);
        Assert.Null(stored.RespondedAt);
    }

    [Fact]
    public async Task CancelInvitation_ShouldPreserveNotification()
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

        await ExecuteAsync(invitation.Id, owner.Id);

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
    public async Task FailedSave_ShouldRollbackCancellation()
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
                .GetRequiredService<CancelOrganizationInvitation>();

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
                    owner.Id,
                    CancellationToken.None));
        }

        await AssertPendingAsync(invitation.Id);
    }

    [Fact]
    public async Task ConcurrentCancellations_ShouldAllowOnlyOneSuccess()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await CreateInvitationAsync(
            organization,
            owner,
            recipient);

        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<Exception?> TryCancelAsync()
        {
            await start.Task;

            try
            {
                await ExecuteAsync(invitation.Id, owner.Id);
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        var firstTask = TryCancelAsync();
        var secondTask = TryCancelAsync();

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

        Assert.Equal(OrganizationInvitationStatus.Cancelled, stored.Status);
        Assert.Null(stored.RespondedAt);

        Assert.False(await dbContext.OrganizationMembers
            .AnyAsync(member =>
                member.OrganizationId == organization.Id &&
                member.UserId == recipient.Id));
    }

    [Fact]
    public async Task ConcurrentAcceptAndCancel_ShouldHaveOneTerminalOutcome()
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
                using var scope = factory.Services.CreateScope();

                var useCase = scope.ServiceProvider
                    .GetRequiredService<AcceptOrganizationInvitation>();

                await useCase.ExecuteAsync(
                    invitation.Id,
                    recipient.Id,
                    CancellationToken.None);

                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        async Task<Exception?> TryCancelAsync()
        {
            await start.Task;

            try
            {
                await ExecuteAsync(invitation.Id, owner.Id);
                return null;
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        var acceptTask = TryAcceptAsync();
        var cancelTask = TryCancelAsync();

        start.SetResult();

        var results = await Task.WhenAll(acceptTask, cancelTask);

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

        var members = await dbContext.OrganizationMembers
            .AsNoTracking()
            .Where(member =>
                member.OrganizationId == organization.Id &&
                member.UserId == recipient.Id)
            .ToListAsync();

        if (stored.Status == OrganizationInvitationStatus.Accepted)
        {
            var member = Assert.Single(members);

            Assert.Equal(OrganizationMemberStatus.Active, member.Status);
            Assert.Equal(invitation.RoleId, member.RoleId);
            Assert.NotNull(stored.RespondedAt);
        }
        else
        {
            Assert.Equal(OrganizationInvitationStatus.Cancelled, stored.Status);
            Assert.Empty(members);
            Assert.Null(stored.RespondedAt);
        }
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = new Organization(
            owner.Id,
            "Invitation Cancellation Organization",
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
            $"invitation-cancellation-{Guid.NewGuid():N}",
            $"invitation-cancellation-{Guid.NewGuid():N}@example.com",
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
            .GetRequiredService<CancelOrganizationInvitation>();

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