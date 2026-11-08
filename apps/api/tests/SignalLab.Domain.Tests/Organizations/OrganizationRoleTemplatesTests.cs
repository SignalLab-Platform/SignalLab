using SignalLab.Domain.Organizations;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationRoleTemplatesTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ShouldCreateThreeSystemRoles()
    {
        var organizationId = OrganizationId.New();

        var roles = OrganizationRoleTemplates.Create(
            organizationId,
            CreatedAt);

        Assert.Equal(3, roles.Count);
        Assert.Equal(
            new[] { "Owner", "Administrator", "Member" },
            roles.Select(role => role.Name));

        Assert.All(roles, role =>
        {
            Assert.Equal(organizationId, role.OrganizationId);
            Assert.Equal(CreatedAt, role.CreatedAt);
            Assert.Equal(CreatedAt, role.UpdatedAt);
            Assert.NotEmpty(role.Permissions);
        });
    }

    [Fact]
    public void Create_ShouldDefineExactlyOneOwnerRole()
    {
        var roles = OrganizationRoleTemplates.Create(
            OrganizationId.New(),
            CreatedAt);

        var owner = Assert.Single(
            roles,
            role => role.IsOwner);

        Assert.Equal("Owner", owner.Name);
    }

    [Fact]
    public void Create_ShouldAssignExpectedMemberPermissions()
    {
        var member = GetRole("Member");

        Assert.Equal(
            new[]
            {
                Permission.ManageProjects,
                Permission.ManageCampaigns,
                Permission.ManageTemplates,
                Permission.ManageMeasures,
                Permission.ViewAnalytics,
                Permission.ExportData
            }.OrderBy(permission => permission),
            member.Permissions.OrderBy(permission => permission));
    }

    [Fact]
    public void Create_ShouldAssignExpectedAdministratorPermissions()
    {
        var administrator = GetRole("Administrator");

        var expectedPermissions = new[]
        {
            Permission.ManageProjects,
            Permission.ManageCampaigns,
            Permission.ManageTemplates,
            Permission.ManageMeasures,
            Permission.ViewAnalytics,
            Permission.ExportData,
            Permission.ManageOrganization,
            Permission.ManageMembers
        };

        Assert.Equal(
            expectedPermissions.OrderBy(permission => permission),
            administrator.Permissions.OrderBy(permission => permission));

        Assert.DoesNotContain(
            Permission.ManageOwnership,
            administrator.Permissions);
    }

    [Fact]
    public void Create_ShouldAssignAllPermissionsToOwner()
    {
        var owner = GetRole("Owner");

        Assert.Equal(
            Enum.GetValues<Permission>().OrderBy(permission => permission),
            owner.Permissions.OrderBy(permission => permission));
    }

    [Fact]
    public void Create_ShouldGenerateDistinctRoleIds()
    {
        var roles = OrganizationRoleTemplates.Create(
            OrganizationId.New(),
            CreatedAt);

        Assert.Equal(
            roles.Count,
            roles.Select(role => role.Id).Distinct().Count());
    }

    private static Role GetRole(string name)
    {
        return OrganizationRoleTemplates
            .Create(OrganizationId.New(), CreatedAt)
            .Single(role => role.Name == name);
    }
}