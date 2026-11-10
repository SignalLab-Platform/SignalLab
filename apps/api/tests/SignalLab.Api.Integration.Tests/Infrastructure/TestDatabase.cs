using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Notifications;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.Api.Integration.Tests.Infrastructure;

public sealed class TestDatabase
{
    private readonly IServiceProvider services;

    public TestDatabase(IServiceProvider services)
    {
        this.services = services;
    }

    public async Task InitializeAsync()
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetUserAsync(string externalIdentityId)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var userId = await dbContext.Users
            .Where(user =>
                user.ExternalIdentityId == externalIdentityId)
            .Select(user => (UserId?)user.Id)
            .SingleOrDefaultAsync();

        if (userId is null)
        {
            return;
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        var organizationIds = await dbContext.OrganizationMembers
            .Where(member => member.UserId == userId.Value)
            .Select(member => member.OrganizationId)
            .Distinct()
            .ToListAsync();

        await DeleteInvitationsAsync(
            dbContext,
            organizationIds,
            new[] { userId.Value });

        await dbContext.Notifications
            .Where(notification => notification.UserId == userId.Value)
            .ExecuteDeleteAsync();

        if (organizationIds.Count > 0)
        {
            await dbContext.OrganizationMembers
                .Where(member =>
                    organizationIds.Contains(member.OrganizationId))
                .ExecuteDeleteAsync();

            await dbContext.Roles
                .Where(role =>
                    organizationIds.Contains(role.OrganizationId))
                .ExecuteDeleteAsync();

            await dbContext.Organizations
                .Where(organization =>
                    organizationIds.Contains(organization.Id))
                .ExecuteDeleteAsync();
        }

        await dbContext.Users
            .Where(user => user.Id == userId.Value)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    user => user.AvatarFileId,
                    (StoredFileId?)null));

        await dbContext.StoredFiles
            .Where(file => file.OwnerUserId == userId.Value)
            .ExecuteDeleteAsync();

        await dbContext.Users
            .Where(user => user.Id == userId.Value)
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }
 
    public async Task ResetUsersAsync(
        IReadOnlyCollection<UserId> userIds)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        await DeleteInvitationsAsync(
            dbContext,
            Array.Empty<OrganizationId>(),
            userIds);

        await dbContext.Notifications
            .Where(notification => userIds.Contains(notification.UserId))
            .ExecuteDeleteAsync();

        await dbContext.Users
            .Where(user => userIds.Contains(user.Id))
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task<User?> GetUserAsync(string externalIdentityId)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        return await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.ExternalIdentityId == externalIdentityId);
    }

    public async Task<StoredFile?> GetStoredFileAsync(
        StoredFileId storedFileId)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        return await dbContext.StoredFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                storedFile => storedFile.Id == storedFileId);
    }
    
    public async Task ResetOrganizationsAsync(
        IReadOnlyCollection<OrganizationId> organizationIds)
    {
        if (organizationIds.Count == 0)
        {
            return;
        }

        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        await DeleteInvitationsAsync(
            dbContext,
            organizationIds,
            Array.Empty<UserId>());

        await dbContext.OrganizationMembers
            .Where(member => organizationIds.Contains(member.OrganizationId))
            .ExecuteDeleteAsync();

        await dbContext.Roles
            .Where(role => organizationIds.Contains(role.OrganizationId))
            .ExecuteDeleteAsync();

        await dbContext.Organizations
            .Where(organization => organizationIds.Contains(organization.Id))
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }

    public async Task AddOrganizationMemberAsync(
        OrganizationId organizationId,
        UserId userId,
        string roleName)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var organization = await dbContext.Organizations
            .Include(organization => organization.Roles)
            .Include(organization => organization.Members)
            .AsSplitQuery()
            .SingleAsync(organization => organization.Id == organizationId);

        var role = organization.Roles.Single(
            role => role.Name == roleName);

        organization.AddMember(
            userId,
            role.Id,
            DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveOrganizationMemberAsync(
        OrganizationId organizationId,
        UserId userId)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var organization = await dbContext.Organizations
            .Include(organization => organization.Roles)
            .Include(organization => organization.Members)
            .AsSplitQuery()
            .SingleAsync(organization => organization.Id == organizationId);

        organization.RemoveMember(
            userId,
            DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync();
    }

    private static async Task DeleteInvitationsAsync(
        SignalLabDbContext dbContext,
        IReadOnlyCollection<OrganizationId> organizationIds,
        IReadOnlyCollection<UserId> userIds)
    {
        var invitationIds = await dbContext.OrganizationInvitations
            .Where(invitation =>
                organizationIds.Contains(invitation.OrganizationId) ||
                userIds.Contains(invitation.InvitedUserId) ||
                userIds.Contains(invitation.InvitedByUserId))
            .Select(invitation => invitation.Id.Value)
            .ToListAsync();

        if (invitationIds.Count == 0)
        {
            return;
        }

        await dbContext.Notifications
            .Where(notification =>
                notification.Target.Type ==
                    NotificationTargetType.OrganizationInvitation &&
                invitationIds.Contains(notification.Target.Id))
            .ExecuteDeleteAsync();

        await dbContext.OrganizationInvitations
            .Where(invitation =>
                organizationIds.Contains(invitation.OrganizationId) ||
                userIds.Contains(invitation.InvitedUserId) ||
                userIds.Contains(invitation.InvitedByUserId))
            .ExecuteDeleteAsync();
    }
}