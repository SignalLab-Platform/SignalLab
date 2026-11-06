using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateOrganization()
    {
        var createdAt = DateTimeOffset.UtcNow;

        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            "https://signallab.example",
            createdAt);

        Assert.NotEqual(Guid.Empty, organization.Id.Value);
        Assert.Equal("SignalLab Studio", organization.Name);
        Assert.Equal("FR", organization.Country);
        Assert.Equal("https://signallab.example", organization.Website);
        Assert.Equal(createdAt, organization.CreatedAt);
        Assert.Equal(createdAt, organization.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ShouldFail(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organization(
                UserId.New(),
                name,
                "FR",
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyCountry_ShouldFail(string country)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organization(
                UserId.New(),
                "SignalLab Studio",
                country,
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("country", exception.ParamName);
    }

    [Theory]
    [InlineData("FR")]
    [InlineData("DE")]
    [InlineData("GB")]
    [InlineData("BE")]
    [InlineData("US")]
    [InlineData("JP")]
    public void Constructor_WithValidCountryCode_ShouldAcceptCountry(
        string country)
    {
        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            country,
            null,
            DateTimeOffset.UtcNow);

        Assert.Equal(country, organization.Country);
    }

    [Theory]
    [InlineData("France")]
    [InlineData("FRA")]
    [InlineData("ZZ")]
    [InlineData("fr")]
    [InlineData("123")]
    [InlineData("F1")]
    public void Constructor_WithInvalidCountryCode_ShouldFail(
        string country)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organization(
                UserId.New(),
                "SignalLab Studio",
                country,
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("country", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldNormalizeNameAndCountry()
    {
        var organization = new Organization(
            UserId.New(),
            "  SignalLab Studio  ",
            "  FR  ",
            null,
            DateTimeOffset.UtcNow);

        Assert.Equal("SignalLab Studio", organization.Name);
        Assert.Equal("FR", organization.Country);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyWebsite_ShouldSetWebsiteToNull(
        string? website)
    {
        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            website,
            DateTimeOffset.UtcNow);

        Assert.Null(organization.Website);
    }

    [Theory]
    [InlineData("https://signallab.example")]
    [InlineData("http://signallab.example")]
    public void Constructor_WithValidWebsite_ShouldAcceptWebsite(
        string website)
    {
        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            website,
            DateTimeOffset.UtcNow);

        Assert.Equal(website, organization.Website);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("signallab.example")]
    [InlineData("https://")]
    [InlineData("ftp://signallab.example")]
    [InlineData("file:///organization")]
    public void Constructor_WithInvalidWebsite_ShouldFail(
        string website)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organization(
                UserId.New(),
                "SignalLab Studio",
                "FR",
                website,
                DateTimeOffset.UtcNow));

        Assert.Equal("website", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldNormalizeWebsite()
    {
        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            "  https://signallab.example  ",
            DateTimeOffset.UtcNow);

        Assert.Equal(
            "https://signallab.example",
            organization.Website);
    }

    [Fact]
    public void Constructor_ShouldInitializeTimestamps()
    {
        var createdAt = new DateTimeOffset(
            2026,
            8,
            1,
            12,
            0,
            0,
            TimeSpan.Zero);

        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            null,
            createdAt);

        Assert.Equal(createdAt, organization.CreatedAt);
        Assert.Equal(createdAt, organization.UpdatedAt);
    }

    [Fact]
    public void Constructor_ShouldInitializeThreeRolesInExpectedOrder()
    {
        var organization = CreateOrganization();

        Assert.Equal(3, organization.Roles.Count);
        Assert.Equal("Owner", organization.Roles[0].Name);
        Assert.Equal("Administrator", organization.Roles[1].Name);
        Assert.Equal("Member", organization.Roles[2].Name);
    }

    [Fact]
    public void Constructor_ShouldCreateRolesForSameOrganization()
    {
        var organization = CreateOrganization();

        Assert.All(
            organization.Roles,
            role => Assert.Equal(
                organization.Id,
                role.OrganizationId));

        Assert.Equal(
            3,
            organization.Roles.Select(role => role.Id).Distinct().Count());
    }

    [Fact]
    public void Constructor_ShouldCreateExactlyOneOwnerRole()
    {
        var organization = CreateOrganization();

        Assert.Single(
            organization.Roles,
            role => role.IsOwner);

        Assert.Same(
            organization.Roles[0],
            organization.OwnerRole);
    }

    [Fact]
    public void Constructor_ShouldInitializeRolePermissions()
    {
        var organization = CreateOrganization();

        var owner = organization.Roles[0];
        var administrator = organization.Roles[1];
        var member = organization.Roles[2];

        Assert.True(owner.HasPermission(Permission.ManageMembers));
        Assert.True(administrator.HasPermission(Permission.ManageMembers));
        Assert.False(member.HasPermission(Permission.ManageMembers));

        Assert.True(member.HasPermission(Permission.ManageProjects));
        Assert.True(member.HasPermission(Permission.ViewAnalytics));

        Assert.True(
            member.Permissions.All(
                permission => administrator.HasPermission(permission)));

        Assert.True(
            administrator.Permissions.All(
                permission => owner.HasPermission(permission)));
    }

    [Fact]
    public void Constructor_ShouldInitializeRoleTimestamps()
    {
        var createdAt = new DateTimeOffset(
            2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

        var organization = new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            null,
            createdAt);

        Assert.All(
            organization.Roles,
            role =>
            {
                Assert.Equal(createdAt, role.CreatedAt);
                Assert.Equal(createdAt, role.UpdatedAt);
            });
    }
    
    [Fact]
    public void Constructor_ShouldCreateCreatorAsActiveOwner()
    {
        var creatorId = UserId.New();
        var createdAt = DateTimeOffset.UtcNow;

        var organization = new Organization(
            creatorId,
            "SignalLab Studio",
            "FR",
            null,
            createdAt);

        var member = Assert.Single(organization.Members);

        Assert.Equal(creatorId, member.UserId);
        Assert.Equal(organization.Id, member.OrganizationId);
        Assert.Equal(organization.OwnerRole.Id, member.RoleId);
        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(createdAt, member.JoinedAt);
        Assert.Null(member.RemovedAt);
    }

    [Fact]
    public void Constructor_ShouldHaveExactlyOneActiveOwnerMember()
    {
        var organization = CreateOrganization();

        var activeOwners = organization.Members.Where(
            member =>
                member.Status == OrganizationMemberStatus.Active &&
                member.RoleId == organization.OwnerRole.Id);

        Assert.Single(activeOwners);
    }

    [Fact]
    public void Constructor_WithEmptyCreatorId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organization(
                new UserId(Guid.Empty),
                "SignalLab Studio",
                "FR",
                null,
                DateTimeOffset.UtcNow));

        Assert.Equal("creatorId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldAssignCreatorRoleFromOrganization()
    {
        var organization = CreateOrganization();

        var creator = Assert.Single(organization.Members);

        var assignedRole = Assert.Single(
            organization.Roles,
            role => role.Id == creator.RoleId);

        Assert.True(assignedRole.IsOwner);
        Assert.True(
            assignedRole.HasPermission(Permission.ManageOwnership));
    }

    private static Organization CreateOrganization()
    {
        return new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            null,
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldReserveManageOwnershipPermissionForOwner()
    {
        var organization = CreateOrganization();

        var owner = organization.Roles[0];
        var administrator = organization.Roles[1];
        var member = organization.Roles[2];

        Assert.True(owner.IsOwner);
        Assert.True(owner.HasPermission(Permission.ManageOwnership));

        Assert.False(administrator.IsOwner);
        Assert.False(administrator.HasPermission(Permission.ManageOwnership));

        Assert.False(member.IsOwner);
        Assert.False(member.HasPermission(Permission.ManageOwnership));
    }
}
