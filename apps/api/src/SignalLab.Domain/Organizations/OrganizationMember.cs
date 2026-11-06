using SignalLab.Domain.Users;

namespace SignalLab.Domain.Organizations;

public sealed class OrganizationMember
{
    public OrganizationMemberId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public UserId UserId { get; private set; }

    public RoleId RoleId { get; private set; }

    public OrganizationMemberStatus Status { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public DateTimeOffset? RemovedAt { get; private set; }

    private OrganizationMember()
    {
    }

    public OrganizationMember(
        OrganizationId organizationId,
        UserId userId,
        RoleId roleId,
        DateTimeOffset joinedAt)
    {
        if (organizationId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "OrganizationId cannot be empty.",
                nameof(organizationId));
        }

        if (userId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId cannot be empty.",
                nameof(userId));
        }

        if (roleId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(roleId));
        }

        Id = OrganizationMemberId.New();
        OrganizationId = organizationId;
        UserId = userId;
        RoleId = roleId;
        Status = OrganizationMemberStatus.Active;
        JoinedAt = joinedAt;
        RemovedAt = null;
    }

    internal void Remove(DateTimeOffset removedAt)
    {
        Status = OrganizationMemberStatus.Removed;
        RemovedAt = removedAt;
    }

    internal void Reactivate(RoleId roleId)
    {
        RoleId = roleId;
        Status = OrganizationMemberStatus.Active;
        RemovedAt = null;
    }

    internal void ChangeRole(RoleId roleId)
    {
        RoleId = roleId;
    }
}