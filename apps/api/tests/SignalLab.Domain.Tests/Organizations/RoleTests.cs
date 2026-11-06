using SignalLab.Domain.Organizations;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class RoleTests
{
    private static readonly OrganizationId OrganizationId =
        new(Guid.NewGuid());

    private static readonly DateTimeOffset Now =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_CreatesRoleWithExpectedProperties()
    {
        var role = new Role(
            OrganizationId,
            " Administrator ",
            " Manages members ",
            false,
            new[] { Permission.ManageMembers, Permission.ManageProjects },
            Now);

        Assert.NotEqual(Guid.Empty, role.Id.Value);
        Assert.Equal(OrganizationId, role.OrganizationId);
        Assert.Equal("Administrator", role.Name);
        Assert.Equal("Manages members", role.Description);
        Assert.False(role.IsOwner);
        Assert.Equal(Now, role.CreatedAt);
        Assert.Equal(Now, role.UpdatedAt);
        Assert.Equal(2, role.Permissions.Count);
    }

    [Fact]
    public void Constructor_RemovesDuplicatePermissions()
    {
        var role = CreateRole(
            Permission.ManageMembers,
            Permission.ManageMembers);

        Assert.Single(role.Permissions);
    }

    [Fact]
    public void Constructor_RejectsUnknownPermission()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateRole((Permission)999));
    }

    [Fact]
    public void Constructor_RejectsEmptyPermissions()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateRole());
    }

    [Fact]
    public void Constructor_RejectsEmptyName()
    {
        Assert.Throws<ArgumentException>(() =>
            new Role(
                OrganizationId,
                " ",
                null,
                false,
                new[] { Permission.ManageMembers },
                Now));
    }

    [Fact]
    public void Constructor_RejectsEmptyOrganizationId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Role(
                new OrganizationId(Guid.Empty),
                "Member",
                null,
                false,
                new[] { Permission.ViewAnalytics },
                Now));
    }

    [Fact]
    public void HasPermission_ReturnsTrueForGrantedPermission()
    {
        var role = CreateRole(Permission.ManageMembers);

        Assert.True(role.HasPermission(Permission.ManageMembers));
    }

    [Fact]
    public void HasPermission_ReturnsFalseForMissingPermission()
    {
        var role = CreateRole(Permission.ManageMembers);

        Assert.False(role.HasPermission(Permission.ExportData));
    }

    private static Role CreateRole(params Permission[] permissions)
    {
        return new Role(
            OrganizationId,
            "Member",
            null,
            false,
            permissions,
            Now);
    }
}
