using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Organizations;

public sealed class OrganizationAccessServiceTests
{
    [Fact]
    public async Task GetAccessibleAsync_WithActiveMember_ReturnsOrganization()
    {
        var owner = UserId.New();
        var organization = CreateOrganization(owner);
        var repository = new FakeOrganizationRepository(organization);
        var service = new OrganizationAccessService(repository);

        var result = await service.GetAccessibleAsync(
            organization.Id,
            owner,
            CancellationToken.None);

        Assert.Same(organization, result);
    }

    [Fact]
    public async Task GetAccessibleAsync_WithRemovedMember_ReturnsNull()
    {
        var owner = UserId.New();
        var member = UserId.New();
        var organization = CreateOrganization(owner);

        var memberRole = organization.Roles.Single(
            role => role.Name == "Member");

        organization.AddMember(
            member,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            member,
            DateTimeOffset.UtcNow);

        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(organization));

        var result = await service.GetAccessibleAsync(
            organization.Id,
            member,
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccessibleAsync_WithExternalUser_ReturnsNull()
    {
        var organization = CreateOrganization(UserId.New());

        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(organization));

        var result = await service.GetAccessibleAsync(
            organization.Id,
            UserId.New(),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccessibleAsync_WithUnknownOrganization_ReturnsNull()
    {
        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(null));

        var result = await service.GetAccessibleAsync(
            OrganizationId.New(),
            UserId.New(),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public void HasPermission_WithOwner_ReturnsTrue()
    {
        var owner = UserId.New();
        var organization = CreateOrganization(owner);

        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(organization));

        Assert.True(service.HasPermission(
            organization,
            owner,
            Permission.ManageMembers));
    }

    [Fact]
    public void HasPermission_WithRegularMember_ReturnsFalse()
    {
        var owner = UserId.New();
        var member = UserId.New();
        var organization = CreateOrganization(owner);

        var memberRole = organization.Roles.Single(
            role => role.Name == "Member");

        organization.AddMember(
            member,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(organization));

        Assert.False(service.HasPermission(
            organization,
            member,
            Permission.ManageMembers));
    }

    [Fact]
    public void HasPermission_WithRemovedAdministrator_ReturnsFalse()
    {
        var owner = UserId.New();
        var administrator = UserId.New();
        var organization = CreateOrganization(owner);

        var administratorRole = organization.Roles.Single(
            role => role.Name == "Administrator");

        organization.AddMember(
            administrator,
            administratorRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            administrator,
            DateTimeOffset.UtcNow);

        var service = new OrganizationAccessService(
            new FakeOrganizationRepository(organization));

        Assert.False(service.HasPermission(
            organization,
            administrator,
            Permission.ManageMembers));
    }

    private static Organization CreateOrganization(UserId owner)
    {
        return new Organization(
            owner,
            "SignalLab Studio",
            "FR",
            null,
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        private readonly Organization? organization;

        public FakeOrganizationRepository(Organization? organization)
        {
            this.organization = organization;
        }

        public Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                organization?.Id == organizationId
                    ? organization
                    : null);
        }

        public void Add(Organization organization)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Organization>> GetAccessibleByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Organization>> GetByIdsAsync(
            IReadOnlyCollection<OrganizationId> organizationIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}