using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationMemberTests
{
    private static readonly DateTimeOffset JoinedAt =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_WithValidValues_ShouldCreateActiveMember()
    {
        var organizationId = new OrganizationId(Guid.NewGuid());
        var userId = UserId.New();
        var roleId = RoleId.New();

        var member = new OrganizationMember(
            organizationId,
            userId,
            roleId,
            JoinedAt);

        Assert.NotEqual(Guid.Empty, member.Id.Value);
        Assert.Equal(organizationId, member.OrganizationId);
        Assert.Equal(userId, member.UserId);
        Assert.Equal(roleId, member.RoleId);
        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(JoinedAt, member.JoinedAt);
        Assert.Null(member.RemovedAt);
    }

    [Fact]
    public void Constructor_ShouldGenerateDistinctMemberIds()
    {
        var organizationId = new OrganizationId(Guid.NewGuid());
        var userId = UserId.New();
        var roleId = RoleId.New();

        var first = new OrganizationMember(
            organizationId,
            userId,
            roleId,
            JoinedAt);

        var second = new OrganizationMember(
            organizationId,
            userId,
            roleId,
            JoinedAt);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Constructor_WithEmptyOrganizationId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new OrganizationMember(
                new OrganizationId(Guid.Empty),
                UserId.New(),
                RoleId.New(),
                JoinedAt));

        Assert.Equal("organizationId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new OrganizationMember(
                new OrganizationId(Guid.NewGuid()),
                new UserId(Guid.Empty),
                RoleId.New(),
                JoinedAt));

        Assert.Equal("userId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyRoleId_ShouldFail()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new OrganizationMember(
                new OrganizationId(Guid.NewGuid()),
                UserId.New(),
                new RoleId(Guid.Empty),
                JoinedAt));

        Assert.Equal("roleId", exception.ParamName);
    }
}
