using SignalLab.Domain.Users;

namespace SignalLab.Domain.Organizations;

public sealed class OrganizationInvitation
{
    public OrganizationInvitationId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public UserId InvitedUserId { get; private set; }

    public UserId InvitedByUserId { get; private set; }

    public RoleId RoleId { get; private set; }

    public OrganizationInvitationStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    private OrganizationInvitation()
    {
    }

    public OrganizationInvitation(
        OrganizationId organizationId,
        UserId invitedUserId,
        UserId invitedByUserId,
        RoleId roleId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (organizationId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "OrganizationId cannot be empty.",
                nameof(organizationId));
        }

        if (invitedUserId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "InvitedUserId cannot be empty.",
                nameof(invitedUserId));
        }

        if (invitedByUserId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "InvitedByUserId cannot be empty.",
                nameof(invitedByUserId));
        }

        if (roleId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(roleId));
        }

        if (expiresAt <= createdAt)
        {
            throw new ArgumentException(
                "ExpiresAt must be later than CreatedAt.",
                nameof(expiresAt));
        }

        Id = OrganizationInvitationId.New();
        OrganizationId = organizationId;
        InvitedUserId = invitedUserId;
        InvitedByUserId = invitedByUserId;
        RoleId = roleId;

        Status = OrganizationInvitationStatus.Pending;

        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        ExpiresAt = expiresAt;
        RespondedAt = null;
    }

    public bool IsExpired(DateTimeOffset now)
    {
        return Status == OrganizationInvitationStatus.Pending
            && now >= ExpiresAt;
    }

    public OrganizationInvitationStatus GetEffectiveStatus(DateTimeOffset now)
    {
        return IsExpired(now)
            ? OrganizationInvitationStatus.Expired
            : Status;
    }

    public void Accept(UserId userId, DateTimeOffset now)
    {
        EnsureRecipient(userId);
        EnsurePendingAndNotExpired(now);

        Status = OrganizationInvitationStatus.Accepted;
        RespondedAt = now;
        UpdatedAt = now;
    }

    public void Decline(UserId userId, DateTimeOffset now)
    {
        EnsureRecipient(userId);
        EnsurePendingAndNotExpired(now);

        Status = OrganizationInvitationStatus.Declined;
        RespondedAt = now;
        UpdatedAt = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        EnsurePendingAndNotExpired(now);

        Status = OrganizationInvitationStatus.Cancelled;
        UpdatedAt = now;
    }

    public void Expire(DateTimeOffset now)
    {
        if (!IsExpired(now))
        {
            throw new InvalidOperationException(
                "Only an expired pending invitation can be marked as expired.");
        }

        Status = OrganizationInvitationStatus.Expired;
        UpdatedAt = now;
    }

    private void EnsureRecipient(UserId userId)
    {
        if (userId != InvitedUserId)
        {
            throw new InvalidOperationException(
                "Only the invited user can respond to this invitation.");
        }
    }

    private void EnsurePendingAndNotExpired(DateTimeOffset now)
    {
        if (Status != OrganizationInvitationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending invitations can be modified.");
        }

        if (now < CreatedAt)
        {
            throw new InvalidOperationException(
                "An invitation cannot be modified before its creation.");
        }

        if (now >= ExpiresAt)
        {
            throw new InvalidOperationException(
                "The invitation has expired.");
        }
    }
}