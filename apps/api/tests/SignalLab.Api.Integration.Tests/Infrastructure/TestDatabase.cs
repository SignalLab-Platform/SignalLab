using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SignalLab.Domain.Files;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
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

        await dbContext.Users
            .Where(user => userIds.Contains(user.Id))
            .ExecuteDeleteAsync();
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
}