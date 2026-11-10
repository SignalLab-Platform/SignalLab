using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Application.Persistence;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Notifications;
using SignalLab.Domain.Notifications;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationInvitationPersistenceTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();

    private readonly List<UserId> createdUserIds = new();

    public OrganizationInvitationPersistenceTests(SignalLabApiFactory factory)
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
    public async Task CreateInvitation_ShouldPersistAndRestoreProperties()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();
        var expiresAt = createdAt.AddDays(7);

        var invitation = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            expiresAt);

        await PersistInvitationAsync(invitation);

        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationInvitationRepository>();

        var restored = await repository.GetByIdAsync(
            invitation.Id,
            CancellationToken.None);

        Assert.NotNull(restored);
        Assert.Equal(invitation.Id, restored.Id);
        Assert.Equal(organization.Id, restored.OrganizationId);
        Assert.Equal(invitedUser.Id, restored.InvitedUserId);
        Assert.Equal(owner.Id, restored.InvitedByUserId);
        Assert.Equal(invitation.RoleId, restored.RoleId);
        Assert.Equal(OrganizationInvitationStatus.Pending, restored.Status);
        Assert.Equal(createdAt, restored.CreatedAt);
        Assert.Equal(createdAt, restored.UpdatedAt);
        Assert.Equal(expiresAt, restored.ExpiresAt);
        Assert.Null(restored.RespondedAt);
    }

    [Fact]
    public async Task AcceptInvitation_ShouldPersistStatusAndResponseDate()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();

        var invitation = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            createdAt.AddDays(7));

        await PersistInvitationAsync(invitation);

        var respondedAt = createdAt.AddMinutes(5);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var trackedInvitation = await repository.GetByIdAsync(
                invitation.Id,
                CancellationToken.None);

            Assert.NotNull(trackedInvitation);

            trackedInvitation.Accept(invitedUser.Id, respondedAt);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var restored = await repository.GetByIdAsync(
                invitation.Id,
                CancellationToken.None);

            Assert.NotNull(restored);
            Assert.Equal(OrganizationInvitationStatus.Accepted, restored.Status);
            Assert.Equal(respondedAt, restored.RespondedAt);
            Assert.Equal(respondedAt, restored.UpdatedAt);
        }
    }

    [Fact]
    public async Task ExpireInvitation_ShouldPersistExpiredStatus()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();
        var expiresAt = createdAt.AddDays(1);

        var invitation = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            expiresAt);

        await PersistInvitationAsync(invitation);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var trackedInvitation = await repository.GetPendingAsync(
                organization.Id,
                invitedUser.Id,
                CancellationToken.None);

            Assert.NotNull(trackedInvitation);

            Assert.Equal(
                OrganizationInvitationStatus.Expired,
                trackedInvitation.GetEffectiveStatus(expiresAt));

            trackedInvitation.Expire(expiresAt);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var restored = await repository.GetByIdAsync(
                invitation.Id,
                CancellationToken.None);

            Assert.NotNull(restored);
            Assert.Equal(OrganizationInvitationStatus.Expired, restored.Status);
            Assert.Equal(expiresAt, restored.UpdatedAt);
            Assert.Null(restored.RespondedAt);
        }
    }

    [Fact]
    public async Task DuplicatePendingInvitation_ShouldBeRejectedByDatabase()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();

        var first = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            createdAt.AddDays(7));

        await PersistInvitationAsync(first);

        var second = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt.AddMinutes(1),
            createdAt.AddDays(8));

        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationInvitationRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        repository.Add(second);

        var exception =
            await Assert.ThrowsAsync<OrganizationInvitationAlreadyPendingException>(
                () => unitOfWork.SaveChangesAsync(CancellationToken.None));

        Assert.IsType<DbUpdateException>(exception.InnerException);
    }

    [Fact]
    public async Task CompletedInvitation_ShouldAllowNewPendingInvitation()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();

        var first = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            createdAt.AddDays(7));

        await PersistInvitationAsync(first);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            var trackedInvitation = await repository.GetByIdAsync(
                first.Id,
                CancellationToken.None);

            Assert.NotNull(trackedInvitation);

            trackedInvitation.Decline(
                invitedUser.Id,
                createdAt.AddMinutes(1));

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        var second = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt.AddMinutes(2),
            createdAt.AddDays(8));

        await PersistInvitationAsync(second);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var pending = await repository.GetPendingAsync(
                organization.Id,
                invitedUser.Id,
                CancellationToken.None);

            Assert.NotNull(pending);
            Assert.Equal(second.Id, pending.Id);
        }
    }

    [Fact]
    public async Task Invitation_WithRoleFromAnotherOrganization_ShouldBeRejected()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var otherOrganization = new Organization(
            owner.Id,
            "Other Invitation Test Organization",
            "FR",
            null,
            UtcNow());

        createdOrganizationIds.Add(otherOrganization.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.Organizations.Add(otherOrganization);

            await dbContext.SaveChangesAsync();
        }

        var invitation = new OrganizationInvitation(
            organization.Id,
            invitedUser.Id,
            owner.Id,
            otherOrganization.OwnerRole.Id,
            UtcNow(),
            UtcNow().AddDays(7));

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            repository.Add(invitation);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => unitOfWork.SaveChangesAsync(CancellationToken.None));
        }
    }

    [Fact]
    public async Task InvitationAndInvalidNotification_ShouldRollbackBoth()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();

        var invitation = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            createdAt.AddDays(7));

        var invalidUserId = new UserId(Guid.NewGuid());

        var notification = new Notification(
            invalidUserId,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                invitation.Id.Value),
            createdAt);

        using (var scope = factory.Services.CreateScope())
        {
            var invitationRepository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var notificationRepository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            invitationRepository.Add(invitation);
            notificationRepository.Add(notification);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            Assert.False(await dbContext.OrganizationInvitations
                .AnyAsync(item => item.Id == invitation.Id));

            Assert.False(await dbContext.Notifications
                .AnyAsync(item => item.Id == notification.Id));
        }
    }

    [Fact]
    public async Task ResetOrganization_ShouldDeleteInvitationsAndNotifications()
    {
        var (organization, owner, invitedUser) = await CreateScenarioAsync();

        var createdAt = UtcNow();

        var invitation = CreateInvitation(
            organization,
            owner,
            invitedUser,
            createdAt,
            createdAt.AddDays(7));

        var notification = new Notification(
            invitedUser.Id,
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                invitation.Id.Value),
            createdAt);

        using (var scope = factory.Services.CreateScope())
        {
            var invitationRepository = scope.ServiceProvider
                .GetRequiredService<IOrganizationInvitationRepository>();

            var notificationRepository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            invitationRepository.Add(invitation);
            notificationRepository.Add(notification);

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        await factory.Database.ResetOrganizationsAsync(
            new[] { organization.Id });

        createdOrganizationIds.Remove(organization.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            Assert.False(await dbContext.Organizations
                .AnyAsync(item => item.Id == organization.Id));

            Assert.False(await dbContext.OrganizationInvitations
                .AnyAsync(item => item.Id == invitation.Id));

            Assert.False(await dbContext.Notifications
                .AnyAsync(item => item.Id == notification.Id));
        }
    }

    [Fact]
    public async Task ConcurrentInvitations_ShouldAllowOnlyOnePendingInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = organization.Roles.Single(role => role.Name == "Member");

        var now = UtcNow();

        var firstInvitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            role.Id,
            now,
            now.AddDays(7));

        var secondInvitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            role.Id,
            now,
            now.AddDays(7));

        using var firstScope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();

        var firstContext = firstScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var secondContext = secondScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var firstUnitOfWork = firstScope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var secondUnitOfWork = secondScope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var firstReady = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var secondReady = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        async Task ExecuteFirstAsync()
        {
            await firstUnitOfWork.ExecuteInTransactionAsync(
                async cancellationToken =>
                {
                    var existing = await firstContext.OrganizationInvitations
                        .AnyAsync(
                            item =>
                                item.OrganizationId == organization.Id &&
                                item.InvitedUserId == recipient.Id &&
                                item.Status == OrganizationInvitationStatus.Pending,
                            cancellationToken);

                    Assert.False(existing);

                    firstReady.SetResult();

                    await secondReady.Task.WaitAsync(
                        TimeSpan.FromSeconds(10),
                        cancellationToken);

                    firstContext.OrganizationInvitations.Add(firstInvitation);

                    await firstUnitOfWork.SaveChangesAsync(cancellationToken);
                },
                CancellationToken.None);
        }

        async Task ExecuteSecondAsync()
        {
            await secondUnitOfWork.ExecuteInTransactionAsync(
                async cancellationToken =>
                {
                    var existing = await secondContext.OrganizationInvitations
                        .AnyAsync(
                            item =>
                                item.OrganizationId == organization.Id &&
                                item.InvitedUserId == recipient.Id &&
                                item.Status == OrganizationInvitationStatus.Pending,
                            cancellationToken);

                    Assert.False(existing);

                    secondReady.SetResult();

                    await firstReady.Task.WaitAsync(
                        TimeSpan.FromSeconds(10),
                        cancellationToken);

                    secondContext.OrganizationInvitations.Add(secondInvitation);

                    await secondUnitOfWork.SaveChangesAsync(cancellationToken);
                },
                CancellationToken.None);
        }

        var results = await Task.WhenAll(
            CaptureResultAsync(ExecuteFirstAsync),
            CaptureResultAsync(ExecuteSecondAsync));

        Assert.Single(results, result => result is null);

        Assert.Single(
            results,
            result => result is OrganizationInvitationAlreadyPendingException);

        using var verificationScope = factory.Services.CreateScope();

        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var invitations = await verificationContext.OrganizationInvitations
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id)
            .ToListAsync();

        Assert.Single(invitations);
        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitations[0].Status);
    }

    private static async Task<Exception?> CaptureResultAsync(
        Func<Task> operation)
    {
        try
        {
            await operation();

            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private async Task<(Organization Organization, User Owner, User InvitedUser)>
        CreateScenarioAsync()
    {
        var now = UtcNow();

        var owner = new User(
            $"invitation-owner-{Guid.NewGuid():N}",
            $"invitation-owner-{Guid.NewGuid():N}@example.com",
            now);

        var invitedUser = new User(
            $"invitation-recipient-{Guid.NewGuid():N}",
            $"invitation-recipient-{Guid.NewGuid():N}@example.com",
            now);

        var organization = new Organization(
            owner.Id,
            "Invitation Test Organization",
            "FR",
            null,
            now);

        createdUserIds.Add(owner.Id);
        createdUserIds.Add(invitedUser.Id);
        createdOrganizationIds.Add(organization.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(owner);
        dbContext.Users.Add(invitedUser);
        dbContext.Organizations.Add(organization);

        await dbContext.SaveChangesAsync();

        return (organization, owner, invitedUser);
    }

    private static OrganizationInvitation CreateInvitation(
        Organization organization,
        User owner,
        User invitedUser,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        var role = organization.Roles.First(role => !role.IsOwner);

        return new OrganizationInvitation(
            organization.Id,
            invitedUser.Id,
            owner.Id,
            role.Id,
            createdAt,
            expiresAt);
    }

    private async Task PersistInvitationAsync(
        OrganizationInvitation invitation)
    {
        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationInvitationRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        repository.Add(invitation);

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}