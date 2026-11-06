using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Infrastructure.Persistence.Repositories;

namespace SignalLab.Api.Integration.Tests.Organizations;

[Collection(SignalLabApiCollection.Name)]
public sealed class OrganizationPersistenceTests
    : IClassFixture<SignalLabApiFactory>, IAsyncLifetime
{
    private readonly SignalLabApiFactory factory;

    private readonly List<OrganizationId> createdOrganizationIds = new();

    private readonly List<UserId> createdUserIds = new();

    public OrganizationPersistenceTests(SignalLabApiFactory factory)
    {
        this.factory = factory;
    }

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        await factory.ResetTestUserAsync();
    }

    public async Task DisposeAsync()
    {
        await factory.Database.ResetOrganizationsAsync(
            createdOrganizationIds);

        await factory.Database.ResetUsersAsync(
            createdUserIds);
    }

    [Fact]
    public async Task CreateOrganization_ShouldPersistAndRestoreAggregate()
    {
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var sessionResponse =
            await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        var user = await factory.GetTestUserAsync();

        Assert.NotNull(user);

        var createdAt = DateTimeOffset.UtcNow;

        var organization = new Organization(
            user.Id,
            "SignalLab Test Organization",
            "FR",
            "https://example.com",
            createdAt);

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationRepository>();

            repository.Add(organization);

            await repository.SaveChangesAsync(CancellationToken.None);
        }

        Organization? restoredOrganization;

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationRepository>();

            restoredOrganization = await repository.GetByIdAsync(
                organization.Id,
                CancellationToken.None);
        }

        Assert.NotNull(restoredOrganization);

        Assert.Equal(organization.Id, restoredOrganization.Id);
        Assert.Equal(organization.Name, restoredOrganization.Name);
        Assert.Equal(organization.Country, restoredOrganization.Country);
        Assert.Equal(organization.Website, restoredOrganization.Website);

        Assert.Equal(3, restoredOrganization.Roles.Count);
        Assert.Single(restoredOrganization.Members);

        foreach (var originalRole in organization.Roles)
        {
            var restoredRole = Assert.Single(
                restoredOrganization.Roles,
                role => role.Id == originalRole.Id);

            Assert.Equal(originalRole.Name, restoredRole.Name);
            Assert.Equal(originalRole.IsOwner, restoredRole.IsOwner);

            Assert.Equal(
                originalRole.Permissions.OrderBy(permission => permission),
                restoredRole.Permissions.OrderBy(permission => permission));
        }

        var ownerMembership = Assert.Single(
            restoredOrganization.Members);

        Assert.Equal(user.Id, ownerMembership.UserId);
        Assert.Equal(
            restoredOrganization.OwnerRole.Id,
            ownerMembership.RoleId);

        Assert.Equal(
            OrganizationMemberStatus.Active,
            ownerMembership.Status);

        Assert.Null(ownerMembership.RemovedAt);
    }

    [Fact]
    public async Task CreateOrganization_WithInvalidMember_ShouldRollbackEntireAggregate()
    {
        var user = await CreateTestUserAsync();

        var organization = new Organization(
            user.Id,
            "Atomicity Test Organization",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        var invalidUserId = new UserId(Guid.NewGuid());

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.Organizations.Add(organization);

            var ownerMembership = Assert.Single(organization.Members);

            dbContext.Entry(ownerMembership)
                .Property(member => member.UserId)
                .CurrentValue = invalidUserId;

            await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync());
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            Assert.False(await dbContext.Organizations
                .AnyAsync(item => item.Id == organization.Id));

            Assert.False(await dbContext.Roles
                .AnyAsync(role => role.OrganizationId == organization.Id));

            Assert.False(await dbContext.OrganizationMembers
                .AnyAsync(member => member.OrganizationId == organization.Id));
        }
    }
    
    [Fact]
    public async Task OrganizationMembers_WithDuplicateUser_ShouldRejectSecondMembership()
    {
        var user = await CreateTestUserAsync();

        var organization = new Organization(
            user.Id,
            "Membership Uniqueness Test",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        await PersistOrganizationAsync(organization);

        var originalMember = Assert.Single(organization.Members);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var duplicateMember = new OrganizationMember(
                organization.Id,
                user.Id,
                organization.OwnerRole.Id,
                DateTimeOffset.UtcNow);

            dbContext.OrganizationMembers.Add(duplicateMember);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync());
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var members = await dbContext.OrganizationMembers
                .AsNoTracking()
                .Where(member => member.OrganizationId == organization.Id)
                .ToListAsync();

            var persistedMember = Assert.Single(members);

            Assert.Equal(originalMember.Id, persistedMember.Id);
            Assert.Equal(user.Id, persistedMember.UserId);
        }
    }
    
    [Fact]
    public async Task OrganizationMember_WithRoleFromAnotherOrganization_ShouldBeRejected()
    {
        var user = await CreateTestUserAsync();

        var firstOrganization = new Organization(
            user.Id,
            "First Organization",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        var secondOrganization = new Organization(
            user.Id,
            "Second Organization",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        await PersistOrganizationAsync(firstOrganization);
        await PersistOrganizationAsync(secondOrganization);

        var firstOwner = Assert.Single(firstOrganization.Members);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var persistedMember = await dbContext.OrganizationMembers
                .SingleAsync(member => member.Id == firstOwner.Id);

            dbContext.Entry(persistedMember)
                .Property(member => member.RoleId)
                .CurrentValue = secondOrganization.OwnerRole.Id;

            await Assert.ThrowsAsync<DbUpdateException>(
                () => dbContext.SaveChangesAsync());
        }

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            var persistedMember = await dbContext.OrganizationMembers
                .AsNoTracking()
                .SingleAsync(member => member.Id == firstOwner.Id);

            Assert.Equal(
                firstOrganization.OwnerRole.Id,
                persistedMember.RoleId);
        }
    }
    
    [Fact]
    public async Task GetAccessibleOrganizations_WhenUserHasNoMembership_ReturnsEmptyList()
    {
        var user = await CreateTestUserAsync();

        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationRepository>();

        var organizations = await repository.GetAccessibleByUserIdAsync(
            user.Id,
            CancellationToken.None);

        Assert.Empty(organizations);
    }
    
    [Fact]
    public async Task GetAccessibleOrganizations_WithMultipleMemberships_ReturnsOrderedOrganizations()
    {
        var user = await CreateTestUserAsync();

        var organizationZ = new Organization(
            user.Id,
            "Zeta Studio",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        var organizationA = new Organization(
            user.Id,
            "Alpha Studio",
            "BE",
            "https://example.com",
            DateTimeOffset.UtcNow);

        await PersistOrganizationAsync(organizationZ);
        await PersistOrganizationAsync(organizationA);

        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationRepository>();

        var organizations = await repository.GetAccessibleByUserIdAsync(
            user.Id,
            CancellationToken.None);

        Assert.Equal(2, organizations.Count);

        Assert.Equal(organizationA.Id, organizations[0].Id);
        Assert.Equal(organizationZ.Id, organizations[1].Id);

        Assert.Equal("Alpha Studio", organizations[0].Name);
        Assert.Equal("Zeta Studio", organizations[1].Name);
    }
    
    [Fact]
    public async Task GetAccessibleOrganizations_WithAnotherUserOrganization_ExcludesIt()
    {
        var currentUser = await CreateTestUserAsync();

        var otherUser = new User(
            $"external-other-{Guid.NewGuid():N}",
            $"other-{Guid.NewGuid():N}@example.com",
            DateTimeOffset.UtcNow);

        createdUserIds.Add(otherUser.Id);

        var otherOrganization = new Organization(
            otherUser.Id,
            "Private Organization",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        createdOrganizationIds.Add(otherOrganization.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.Users.Add(otherUser);
            dbContext.Organizations.Add(otherOrganization);

            await dbContext.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationRepository>();

            var organizations = await repository.GetAccessibleByUserIdAsync(
                currentUser.Id,
                CancellationToken.None);

            Assert.DoesNotContain(
                organizations,
                organization => organization.Id == otherOrganization.Id);
        }
    }

    [Fact]
    public async Task GetAccessibleOrganizations_WithRemovedMembership_ExcludesOrganization()
    {
        var currentUser = await CreateTestUserAsync();

        var owner = new User(
            $"external-owner-{Guid.NewGuid():N}",
            $"owner-{Guid.NewGuid():N}@example.com",
            DateTimeOffset.UtcNow);

        createdUserIds.Add(owner.Id);

        var organization = new Organization(
            owner.Id,
            "Former Membership Organization",
            "FR",
            null,
            DateTimeOffset.UtcNow);

        createdOrganizationIds.Add(organization.Id);

        var memberRole = organization.Roles.First(
            role => !role.IsOwner);

        organization.AddMember(
            currentUser.Id,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            currentUser.Id,
            DateTimeOffset.UtcNow);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<SignalLabDbContext>();

            dbContext.Users.Add(owner);
            dbContext.Organizations.Add(organization);

            await dbContext.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider
                .GetRequiredService<IOrganizationRepository>();

            var organizations = await repository.GetAccessibleByUserIdAsync(
                currentUser.Id,
                CancellationToken.None);

            Assert.DoesNotContain(
                organizations,
                item => item.Id == organization.Id);
        }
    }

    private async Task<User> CreateTestUserAsync()
    {
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var token = factory.TokenFactory.CreateToken(
            SignalLabApiFactory.TestIssuer,
            SignalLabApiFactory.TestAuthorizedParty,
            SignalLabApiFactory.TestExternalUserId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response =
            await client.GetAsync("/authentication/session");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await factory.GetTestUserAsync();

        Assert.NotNull(user);

        return user;
    }

    private async Task PersistOrganizationAsync(Organization organization)
    {
        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IOrganizationRepository>();

        repository.Add(organization);

        await repository.SaveChangesAsync(CancellationToken.None);
    }
}