using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Notifications;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Persistence;
using SignalLab.Application.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class CreateOrganizationInvitationTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();
    private readonly List<UserId> createdUserIds = new();

    public CreateOrganizationInvitationTests(SignalLabApiFactory factory)
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
    public async Task CreateInvitation_ShouldPersistInvitationAndNotification()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = GetRole(organization, "Member");

        var invitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var storedInvitation = await dbContext.OrganizationInvitations
            .SingleAsync(item => item.Id == invitation.Id);

        var notification = await dbContext.Notifications
            .SingleAsync(item =>
                item.Target.Type ==
                    NotificationTargetType.OrganizationInvitation &&
                item.Target.Id == invitation.Id.Value);

        Assert.Equal(OrganizationInvitationStatus.Pending, storedInvitation.Status);
        Assert.Equal(organization.Id, storedInvitation.OrganizationId);
        Assert.Equal(owner.Id, storedInvitation.InvitedByUserId);
        Assert.Equal(recipient.Id, storedInvitation.InvitedUserId);
        Assert.Equal(role.Id, storedInvitation.RoleId);

        Assert.Equal(recipient.Id, notification.UserId);
        Assert.Equal(
            NotificationType.OrganizationInvitationReceived,
            notification.Type);
        Assert.Null(notification.ReadAt);
    }

    [Fact]
    public async Task Administrator_ShouldNotBeAllowedToInviteOwner()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var administrator = await CreateUserAsync();

        var administratorRole = GetRole(organization, "Administrator");

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var trackedOrganization = await dbContext.Organizations
                .Include(item => item.Roles)
                .Include(item => item.Members)
                .SingleAsync(item => item.Id == organization.Id);

            trackedOrganization.AddMember(
                administrator.Id,
                administratorRole.Id,
                UtcNow());

            await dbContext.SaveChangesAsync();
        }

        var exception = await Assert.ThrowsAsync<PermissionDeniedException>(
            () => ExecuteAsync(
                organization.Id,
                administrator.Id,
                recipient.Id,
                organization.OwnerRole.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.OwnerInvitationDenied,
            exception.Code);
    }

    [Fact]
    public async Task Owner_ShouldBeAllowedToInviteAnotherOwner()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            organization.OwnerRole.Id);

        Assert.Equal(organization.OwnerRole.Id, invitation.RoleId);
        Assert.Equal(owner.Id, invitation.InvitedByUserId);
    }

    [Fact]
    public async Task MemberWithoutManageMembers_ShouldBeRejected()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var member = await CreateUserAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var trackedOrganization = await dbContext.Organizations
                .Include(item => item.Roles)
                .Include(item => item.Members)
                .SingleAsync(item => item.Id == organization.Id);

            trackedOrganization.AddMember(
                member.Id,
                GetRole(organization, "Member").Id,
                UtcNow());

            await dbContext.SaveChangesAsync();
        }

        var exception = await Assert.ThrowsAsync<PermissionDeniedException>(
            () => ExecuteAsync(
                organization.Id,
                member.Id,
                recipient.Id,
                organization.OwnerRole.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.ManageMembersDenied,
            exception.Code);
    }

    [Fact]
    public async Task UnknownRecipient_ShouldBeRejected()
    {
        var (organization, owner, _) = await CreateScenarioAsync();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                new UserId(Guid.NewGuid()),
                GetRole(organization, "Member").Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.RecipientUnavailable,
            exception.Code);
    }

    [Fact]
    public async Task RoleFromAnotherOrganization_ShouldBeRejected()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var otherOrganization = new Organization(
            owner.Id,
            "Other Invitation Organization",
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

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                recipient.Id,
                otherOrganization.OwnerRole.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidRole,
            exception.Code);
    }

    [Fact]
    public async Task ActiveMember_ShouldNotReceiveInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var trackedOrganization = await dbContext.Organizations
                .Include(item => item.Roles)
                .Include(item => item.Members)
                .SingleAsync(item => item.Id == organization.Id);

            trackedOrganization.AddMember(
                recipient.Id,
                GetRole(organization, "Member").Id,
                UtcNow());

            await dbContext.SaveChangesAsync();
        }

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                recipient.Id,
                GetRole(organization, "Member").Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.AlreadyMember,
            exception.Code);
    }

    [Fact]
    public async Task RemovedMember_ShouldBeAllowedToReceiveInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var trackedOrganization = await dbContext.Organizations
                .Include(item => item.Roles)
                .Include(item => item.Members)
                .SingleAsync(item => item.Id == organization.Id);

            var now = UtcNow();

            trackedOrganization.AddMember(
                recipient.Id,
                GetRole(organization, "Member").Id,
                now);

            trackedOrganization.RemoveMember(
                recipient.Id,
                now.AddMinutes(1));

            await dbContext.SaveChangesAsync();
        }

        var invitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            GetRole(organization, "Member").Id);

        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public async Task ExistingPendingInvitation_ShouldRejectDuplicate()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = GetRole(organization, "Member");

        var firstInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        await Assert.ThrowsAsync<OrganizationInvitationAlreadyPendingException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                recipient.Id,
                role.Id));

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var invitationCount = await dbContext.OrganizationInvitations
            .CountAsync(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id);

        Assert.Equal(1, invitationCount);
        Assert.NotEqual(Guid.Empty, firstInvitation.Id.Value);
    }

    [Theory]
    [InlineData(OrganizationInvitationDuration.OneDay, 1)]
    [InlineData(OrganizationInvitationDuration.SevenDays, 7)]
    [InlineData(OrganizationInvitationDuration.ThirtyDays, 30)]
    public async Task Duration_ShouldDetermineExpiration(
        OrganizationInvitationDuration duration,
        int expectedDays)
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var invitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            GetRole(organization, "Member").Id,
            duration);

        Assert.Equal(
            TimeSpan.FromDays(expectedDays),
            invitation.ExpiresAt - invitation.CreatedAt);
    }

    [Fact]
    public async Task ExpiredInvitation_ShouldBeReplaced()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = GetRole(organization, "Member");

        var oldCreatedAt = UtcNow().AddDays(-10);

        var oldInvitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            role.Id,
            oldCreatedAt,
            oldCreatedAt.AddDays(7));

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.OrganizationInvitations.Add(oldInvitation);

            await dbContext.SaveChangesAsync();
        }

        var newInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var invitations = await dbContext.OrganizationInvitations
                .Where(item =>
                    item.OrganizationId == organization.Id &&
                    item.InvitedUserId == recipient.Id)
                .ToListAsync();

            Assert.Equal(2, invitations.Count);

            var restoredOldInvitation = invitations.Single(
                item => item.Id == oldInvitation.Id);

            Assert.Equal(
                OrganizationInvitationStatus.Expired,
                restoredOldInvitation.Status);

            Assert.Equal(
                oldInvitation.OrganizationId,
                restoredOldInvitation.OrganizationId);

            Assert.Equal(
                oldInvitation.InvitedUserId,
                restoredOldInvitation.InvitedUserId);

            Assert.Equal(
                oldInvitation.RoleId,
                restoredOldInvitation.RoleId);

            Assert.Equal(
                oldInvitation.CreatedAt,
                restoredOldInvitation.CreatedAt);

            Assert.Equal(
                oldInvitation.ExpiresAt,
                restoredOldInvitation.ExpiresAt);

            Assert.Null(restoredOldInvitation.RespondedAt);

            Assert.Contains(invitations, item =>
                item.Id == oldInvitation.Id &&
                item.Status == OrganizationInvitationStatus.Expired);

            Assert.Contains(invitations, item =>
                item.Id == newInvitation.Id &&
                item.Status == OrganizationInvitationStatus.Pending);

            var notificationCount = await dbContext.Notifications
                .CountAsync(item =>
                    item.Target.Type ==
                        NotificationTargetType.OrganizationInvitation &&
                    item.Target.Id == newInvitation.Id.Value);

            Assert.Equal(1, notificationCount);

            var notifications = await dbContext.Notifications
                .AsNoTracking()
                .Where(item =>
                    item.UserId == recipient.Id &&
                    item.Type == NotificationType.OrganizationInvitationReceived)
                .ToListAsync();

            var newNotification = Assert.Single(notifications);

            Assert.Equal(
                NotificationTargetType.OrganizationInvitation,
                newNotification.Target.Type);

            Assert.Equal(
                newInvitation.Id.Value,
                newNotification.Target.Id);

            Assert.NotEqual(
                oldInvitation.Id.Value,
                newNotification.Target.Id);
        }
    }

    [Fact]
    public async Task Transaction_ShouldRollbackFirstSaveWhenSecondSaveFails()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = GetRole(organization, "Member");

        var oldCreatedAt = UtcNow().AddDays(-10);

        var oldInvitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            role.Id,
            oldCreatedAt,
            oldCreatedAt.AddDays(7));

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.OrganizationInvitations.Add(oldInvitation);

            await dbContext.SaveChangesAsync();
        }

        var newInvitation = new OrganizationInvitation(
            organization.Id,
            recipient.Id,
            owner.Id,
            role.Id,
            UtcNow(),
            UtcNow().AddDays(7));

        var invalidNotification = new Notification(
            new UserId(Guid.NewGuid()),
            NotificationType.OrganizationInvitationReceived,
            new NotificationTarget(
                NotificationTargetType.OrganizationInvitation,
                newInvitation.Id.Value),
            UtcNow());

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            await Assert.ThrowsAsync<DbUpdateException>(
                () => unitOfWork.ExecuteInTransactionAsync(
                    async cancellationToken =>
                    {
                        var trackedInvitation =
                            await dbContext.OrganizationInvitations
                                .SingleAsync(
                                    item => item.Id == oldInvitation.Id,
                                    cancellationToken);

                        trackedInvitation.Expire(UtcNow());

                        await unitOfWork.SaveChangesAsync(cancellationToken);

                        dbContext.OrganizationInvitations.Add(newInvitation);
                        dbContext.Notifications.Add(invalidNotification);

                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    },
                    CancellationToken.None));
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var restoredInvitation = await dbContext.OrganizationInvitations
                .SingleAsync(item => item.Id == oldInvitation.Id);

            Assert.Equal(
                OrganizationInvitationStatus.Pending,
                restoredInvitation.Status);

            Assert.False(await dbContext.OrganizationInvitations
                .AnyAsync(item => item.Id == newInvitation.Id));

            Assert.False(await dbContext.Notifications
                .AnyAsync(item => item.Id == invalidNotification.Id));
        }
    }

    [Fact]
    public async Task InvalidDuration_ShouldBeRejectedWithoutCreatingInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                recipient.Id,
                GetRole(organization, "Member").Id,
                (OrganizationInvitationDuration)999));

        Assert.Equal(
            OrganizationInvitationErrorCodes.InvalidDuration,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        Assert.False(await dbContext.OrganizationInvitations
            .AnyAsync(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id));
    }

    [Fact]
    public async Task NonMember_ShouldNotBeAllowedToInvite()
    {
        var (organization, _, recipient) = await CreateScenarioAsync();
        var outsider = await CreateUserAsync();

        var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => ExecuteAsync(
                organization.Id,
                outsider.Id,
                recipient.Id,
                GetRole(organization, "Member").Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.OrganizationNotFound,
            exception.Code);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        Assert.False(await dbContext.OrganizationInvitations
            .AnyAsync(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id));

        Assert.False(await dbContext.Notifications
            .AnyAsync(item => item.UserId == recipient.Id));
    }

    [Fact]
    public async Task DatabasePendingInvitationConflict_ShouldBecomeBusinessException()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();
        var role = GetRole(organization, "Member");
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

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.OrganizationInvitations.Add(firstInvitation);

            await dbContext.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var unitOfWork = scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

            dbContext.OrganizationInvitations.Add(secondInvitation);

            var exception =
                await Assert.ThrowsAsync<OrganizationInvitationAlreadyPendingException>(
                    () => unitOfWork.SaveChangesAsync(CancellationToken.None));

            Assert.IsType<DbUpdateException>(exception.InnerException);
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var invitations = await dbContext.OrganizationInvitations
                .Where(item =>
                    item.OrganizationId == organization.Id &&
                    item.InvitedUserId == recipient.Id)
                .ToListAsync();

            Assert.Single(invitations);
            Assert.Equal(firstInvitation.Id, invitations[0].Id);
        }
    }

    [Fact]
    public async Task CancelledInvitation_ShouldAllowNewInvitation()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var role = GetRole(organization, "Member");

        var firstInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var cancelInvitation = scope.ServiceProvider
                .GetRequiredService<CancelOrganizationInvitation>();

            await cancelInvitation.ExecuteAsync(
                firstInvitation.Id,
                owner.Id,
                CancellationToken.None);
        }

        var secondInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using var verificationScope = factory.Services.CreateScope();

        var dbContext = verificationScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var invitations = await dbContext.OrganizationInvitations
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id)
            .ToListAsync();

        Assert.Equal(2, invitations.Count);

        Assert.Contains(invitations, item =>
            item.Id == firstInvitation.Id &&
            item.Status == OrganizationInvitationStatus.Cancelled);

        Assert.Contains(invitations, item =>
            item.Id == secondInvitation.Id &&
            item.Status == OrganizationInvitationStatus.Pending);

        var notifications = await dbContext.Notifications
            .AsNoTracking()
            .Where(item =>
                item.UserId == recipient.Id &&
                item.Type == NotificationType.OrganizationInvitationReceived)
            .ToListAsync();

        Assert.Equal(2, notifications.Count);

        Assert.Contains(notifications, item =>
            item.Target.Id == firstInvitation.Id.Value);

        Assert.Contains(notifications, item =>
            item.Target.Id == secondInvitation.Id.Value);
    }

    [Fact]
    public async Task AcceptedInvitation_ShouldAllowReinvitationOnlyAfterMemberRemoval()
    {
        var (organization, owner, recipient) = await CreateScenarioAsync();

        var role = GetRole(organization, "Member");

        var firstInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var acceptInvitation = scope.ServiceProvider
                .GetRequiredService<AcceptOrganizationInvitation>();

            await acceptInvitation.ExecuteAsync(
                firstInvitation.Id,
                recipient.Id,
                CancellationToken.None);
        }

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => ExecuteAsync(
                organization.Id,
                owner.Id,
                recipient.Id,
                role.Id));

        Assert.Equal(
            OrganizationInvitationErrorCodes.AlreadyMember,
            exception.Code);

        OrganizationMemberId originalMembershipId;

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var member = await dbContext.OrganizationMembers
                .AsNoTracking()
                .SingleAsync(item =>
                    item.OrganizationId == organization.Id &&
                    item.UserId == recipient.Id);

            originalMembershipId = member.Id;

            Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        }

        await factory.Database.RemoveOrganizationMemberAsync(
            organization.Id,
            recipient.Id);

        var secondInvitation = await ExecuteAsync(
            organization.Id,
            owner.Id,
            recipient.Id,
            role.Id);

        using var verificationScope = factory.Services.CreateScope();

        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var invitations = await verificationContext.OrganizationInvitations
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.InvitedUserId == recipient.Id)
            .ToListAsync();

        Assert.Equal(2, invitations.Count);

        Assert.Contains(invitations, item =>
            item.Id == firstInvitation.Id &&
            item.Status == OrganizationInvitationStatus.Accepted);

        Assert.Contains(invitations, item =>
            item.Id == secondInvitation.Id &&
            item.Status == OrganizationInvitationStatus.Pending);

        var memberships = await verificationContext.OrganizationMembers
            .AsNoTracking()
            .Where(item =>
                item.OrganizationId == organization.Id &&
                item.UserId == recipient.Id)
            .ToListAsync();

        var membership = Assert.Single(memberships);

        Assert.Equal(originalMembershipId, membership.Id);
        Assert.Equal(OrganizationMemberStatus.Removed, membership.Status);
    }

    private async Task<(Organization Organization, User Owner, User Recipient)>
        CreateScenarioAsync()
    {
        var owner = await CreateUserAsync();
        var recipient = await CreateUserAsync();

        var organization = new Organization(
            owner.Id,
            "Invitation Use Case Organization",
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
            $"invitation-use-case-{Guid.NewGuid():N}",
            $"invitation-use-case-{Guid.NewGuid():N}@example.com",
            UtcNow());

        createdUserIds.Add(user.Id);

        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<OrganizationInvitation> ExecuteAsync(
        OrganizationId organizationId,
        UserId invitedByUserId,
        UserId invitedUserId,
        RoleId roleId,
        OrganizationInvitationDuration duration =
            OrganizationInvitationDuration.SevenDays)
    {
        using var scope = factory.Services.CreateScope();

        var useCase = scope.ServiceProvider
            .GetRequiredService<CreateOrganizationInvitation>();

        return await useCase.ExecuteAsync(
            organizationId,
            invitedByUserId,
            invitedUserId,
            roleId,
            duration,
            CancellationToken.None);
    }

    private static Role GetRole(Organization organization, string name)
    {
        return organization.Roles.Single(role => role.Name == name);
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}