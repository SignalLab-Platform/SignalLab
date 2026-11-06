using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationMembershipTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    private static Organization CreateOrganization()
    {
        return new Organization(
            UserId.New(),
            "SignalLab Studio",
            "FR",
            null,
            CreatedAt);
    }

    private static Role GetOrdinaryRole(Organization organization)
    {
        return organization.Roles.Single(
            role => role.Name == "Member");
    }

    [Fact]
    public void AddMember_ShouldCreateActiveMembership()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var role = GetOrdinaryRole(organization);
        var joinedAt = CreatedAt.AddDays(1);

        var member = organization.AddMember(
            userId,
            role.Id,
            joinedAt);

        Assert.Equal(2, organization.Members.Count);
        Assert.Equal(userId, member.UserId);
        Assert.Equal(role.Id, member.RoleId);
        Assert.Equal(OrganizationMemberStatus.Active, member.Status);
        Assert.Equal(joinedAt, member.JoinedAt);
        Assert.Equal(joinedAt, organization.UpdatedAt);
    }

    [Fact]
    public void AddMember_ShouldRejectDuplicateMembership()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var roleId = GetOrdinaryRole(organization).Id;

        organization.AddMember(
            userId,
            roleId,
            CreatedAt.AddDays(1));

        Assert.Throws<InvalidOperationException>(() =>
            organization.AddMember(
                userId,
                roleId,
                CreatedAt.AddDays(2)));

        Assert.Equal(2, organization.Members.Count);
    }

    [Fact]
    public void AddMember_ShouldRejectRoleFromAnotherOrganization()
    {
        var organization = CreateOrganization();
        var anotherOrganization = CreateOrganization();

        Assert.Throws<InvalidOperationException>(() =>
            organization.AddMember(
                UserId.New(),
                anotherOrganization.OwnerRole.Id,
                CreatedAt.AddDays(1)));
    }

    [Fact]
    public void RemoveMember_ShouldPreserveMembershipHistory()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var roleId = GetOrdinaryRole(organization).Id;
        var joinedAt = CreatedAt.AddDays(1);
        var removedAt = CreatedAt.AddDays(2);

        var member = organization.AddMember(
            userId,
            roleId,
            joinedAt);

        var originalId = member.Id;

        organization.RemoveMember(userId, removedAt);

        Assert.Equal(originalId, member.Id);
        Assert.Equal(joinedAt, member.JoinedAt);
        Assert.Equal(OrganizationMemberStatus.Removed, member.Status);
        Assert.Equal(removedAt, member.RemovedAt);
        Assert.Contains(member, organization.Members);
    }

    [Fact]
    public void RemoveMember_ShouldRejectAlreadyRemovedMember()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var roleId = GetOrdinaryRole(organization).Id;

        organization.AddMember(
            userId,
            roleId,
            CreatedAt.AddDays(1));

        organization.RemoveMember(
            userId,
            CreatedAt.AddDays(2));

        Assert.Throws<InvalidOperationException>(() =>
            organization.RemoveMember(
                userId,
                CreatedAt.AddDays(3)));
    }

    [Fact]
    public void ReactivateMember_ShouldPreserveIdentityAndAssignNewRole()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var memberRole = GetOrdinaryRole(organization);
        var administratorRole = organization.Roles.Single(
            role => role.Name == "Administrator");

        var member = organization.AddMember(
            userId,
            memberRole.Id,
            CreatedAt.AddDays(1));

        var originalId = member.Id;
        var originalJoinedAt = member.JoinedAt;

        organization.RemoveMember(
            userId,
            CreatedAt.AddDays(2));

        var reactivated = organization.ReactivateMember(
            userId,
            administratorRole.Id,
            CreatedAt.AddDays(3));

        Assert.Same(member, reactivated);
        Assert.Equal(originalId, reactivated.Id);
        Assert.Equal(originalJoinedAt, reactivated.JoinedAt);
        Assert.Equal(administratorRole.Id, reactivated.RoleId);
        Assert.Equal(OrganizationMemberStatus.Active, reactivated.Status);
        Assert.Null(reactivated.RemovedAt);
        Assert.Equal(2, organization.Members.Count);
    }

    [Fact]
    public void ReactivateMember_ShouldRejectActiveMember()
    {
        var organization = CreateOrganization();
        var creator = Assert.Single(organization.Members);

        Assert.Throws<InvalidOperationException>(() =>
            organization.ReactivateMember(
                creator.UserId,
                organization.OwnerRole.Id,
                CreatedAt.AddDays(1)));
    }

    [Fact]
    public void ChangeMemberRole_ShouldUpdateAssignedRole()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var memberRole = GetOrdinaryRole(organization);

        organization.AddMember(
            userId,
            memberRole.Id,
            CreatedAt.AddDays(1));

        organization.ChangeMemberRole(
            userId,
            organization.OwnerRole.Id,
            CreatedAt.AddDays(2));

        var member = organization.Members.Single(
            member => member.UserId == userId);

        Assert.Equal(organization.OwnerRole.Id, member.RoleId);
        Assert.Equal(2, organization.Members.Count);
    }

    [Fact]
    public void ChangeMemberRole_ShouldRejectRemovedMember()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var roleId = GetOrdinaryRole(organization).Id;

        organization.AddMember(
            userId,
            roleId,
            CreatedAt.AddDays(1));

        organization.RemoveMember(
            userId,
            CreatedAt.AddDays(2));

        Assert.Throws<InvalidOperationException>(() =>
            organization.ChangeMemberRole(
                userId,
                organization.OwnerRole.Id,
                CreatedAt.AddDays(3)));
    }

    [Fact]
    public void RemoveMember_ShouldProtectLastActiveOwner()
    {
        var organization = CreateOrganization();
        var owner = Assert.Single(organization.Members);

        Assert.Throws<InvalidOperationException>(() =>
            organization.RemoveMember(
                owner.UserId,
                CreatedAt.AddDays(1)));

        Assert.Equal(OrganizationMemberStatus.Active, owner.Status);
        Assert.Null(owner.RemovedAt);
    }

    [Fact]
    public void ChangeMemberRole_ShouldProtectLastActiveOwner()
    {
        var organization = CreateOrganization();
        var owner = Assert.Single(organization.Members);
        var memberRole = GetOrdinaryRole(organization);

        Assert.Throws<InvalidOperationException>(() =>
            organization.ChangeMemberRole(
                owner.UserId,
                memberRole.Id,
                CreatedAt.AddDays(1)));

        Assert.Equal(organization.OwnerRole.Id, owner.RoleId);
    }

    [Fact]
    public void RemoveMember_ShouldAllowRemovalWhenAnotherOwnerIsActive()
    {
        var organization = CreateOrganization();
        var originalOwner = Assert.Single(organization.Members);
        var secondOwnerId = UserId.New();

        organization.AddMember(
            secondOwnerId,
            organization.OwnerRole.Id,
            CreatedAt.AddDays(1));

        organization.RemoveMember(
            originalOwner.UserId,
            CreatedAt.AddDays(2));

        Assert.Equal(
            OrganizationMemberStatus.Removed,
            originalOwner.Status);

        Assert.Single(
            organization.Members,
            member =>
                member.Status == OrganizationMemberStatus.Active &&
                member.RoleId == organization.OwnerRole.Id);
    }

    [Fact]
    public void ChangeMemberRole_ShouldAllowDemotionWhenAnotherOwnerIsActive()
    {
        var organization = CreateOrganization();
        var originalOwner = Assert.Single(organization.Members);
        var memberRole = GetOrdinaryRole(organization);

        organization.AddMember(
            UserId.New(),
            organization.OwnerRole.Id,
            CreatedAt.AddDays(1));

        organization.ChangeMemberRole(
            originalOwner.UserId,
            memberRole.Id,
            CreatedAt.AddDays(2));

        Assert.Equal(memberRole.Id, originalOwner.RoleId);

        Assert.Single(
            organization.Members,
            member =>
                member.Status == OrganizationMemberStatus.Active &&
                member.RoleId == organization.OwnerRole.Id);
    }

    [Fact]
    public void AddMember_ShouldRejectHistoricalDuplicateAfterRemoval()
    {
        var organization = CreateOrganization();
        var userId = UserId.New();
        var roleId = GetOrdinaryRole(organization).Id;

        organization.AddMember(
            userId,
            roleId,
            CreatedAt.AddDays(1));

        organization.RemoveMember(
            userId,
            CreatedAt.AddDays(2));

        Assert.Throws<InvalidOperationException>(() =>
            organization.AddMember(
                userId,
                roleId,
                CreatedAt.AddDays(3)));
    }
}