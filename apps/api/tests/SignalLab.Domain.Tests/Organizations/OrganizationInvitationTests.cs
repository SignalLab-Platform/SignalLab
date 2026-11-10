using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationInvitationTests
{
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);

    private static readonly UserId InvitedUserId = UserId.New();

    [Fact]
    public void Constructor_ShouldCreatePendingInvitation()
    {
        var invitation = CreateInvitation();

        Assert.NotEqual(Guid.Empty, invitation.Id.Value);
        Assert.Equal(InvitedUserId, invitation.InvitedUserId);
        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
        Assert.Equal(CreatedAt, invitation.CreatedAt);
        Assert.Equal(CreatedAt, invitation.UpdatedAt);
        Assert.Equal(CreatedAt.AddDays(7), invitation.ExpiresAt);
        Assert.Null(invitation.RespondedAt);
    }

    [Fact]
    public void Constructor_ShouldRejectExpirationAtCreation()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateInvitation(CreatedAt));
    }

    [Fact]
    public void Constructor_ShouldRejectExpirationBeforeCreation()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateInvitation(CreatedAt.AddDays(-1)));
    }

    [Fact]
    public void IsExpired_ShouldUseExactExpirationBoundary()
    {
        var invitation = CreateInvitation();

        Assert.False(invitation.IsExpired(
            invitation.ExpiresAt.AddTicks(-1)));

        Assert.True(invitation.IsExpired(
            invitation.ExpiresAt));
    }

    [Fact]
    public void GetEffectiveStatus_ShouldNotMutatePersistedStatus()
    {
        var invitation = CreateInvitation();

        var status = invitation.GetEffectiveStatus(
            invitation.ExpiresAt);

        Assert.Equal(OrganizationInvitationStatus.Expired, status);
        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
        Assert.Equal(CreatedAt, invitation.UpdatedAt);
    }

    [Fact]
    public void Accept_ShouldTransitionToAccepted()
    {
        var invitation = CreateInvitation();
        var now = CreatedAt.AddDays(1);

        invitation.Accept(InvitedUserId, now);

        Assert.Equal(OrganizationInvitationStatus.Accepted, invitation.Status);
        Assert.Equal(now, invitation.RespondedAt);
        Assert.Equal(now, invitation.UpdatedAt);
    }

    [Fact]
    public void Accept_ShouldRejectDifferentUser()
    {
        var invitation = CreateInvitation();

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Accept(UserId.New(), CreatedAt.AddDays(1)));

        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public void Accept_ShouldRejectExpiredInvitation()
    {
        var invitation = CreateInvitation();

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Accept(InvitedUserId, invitation.ExpiresAt));

        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public void Decline_ShouldTransitionToDeclined()
    {
        var invitation = CreateInvitation();
        var now = CreatedAt.AddDays(1);

        invitation.Decline(InvitedUserId, now);

        Assert.Equal(OrganizationInvitationStatus.Declined, invitation.Status);
        Assert.Equal(now, invitation.RespondedAt);
    }

    [Fact]
    public void Decline_ShouldRejectDifferentUser()
    {
        var invitation = CreateInvitation();

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Decline(UserId.New(), CreatedAt.AddDays(1)));

        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public void Cancel_ShouldTransitionToCancelled()
    {
        var invitation = CreateInvitation();
        var now = CreatedAt.AddDays(1);

        invitation.Cancel(now);

        Assert.Equal(OrganizationInvitationStatus.Cancelled, invitation.Status);
        Assert.Null(invitation.RespondedAt);
        Assert.Equal(now, invitation.UpdatedAt);
    }

    [Fact]
    public void Expire_ShouldMaterializeExpiration()
    {
        var invitation = CreateInvitation();
        var now = invitation.ExpiresAt;

        invitation.Expire(now);

        Assert.Equal(OrganizationInvitationStatus.Expired, invitation.Status);
        Assert.Equal(now, invitation.UpdatedAt);
        Assert.Null(invitation.RespondedAt);
    }

    [Fact]
    public void Expire_ShouldRejectPrematureExpiration()
    {
        var invitation = CreateInvitation();

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Expire(CreatedAt.AddDays(1)));
    }

    [Theory]
    [InlineData(OrganizationInvitationStatus.Accepted)]
    [InlineData(OrganizationInvitationStatus.Declined)]
    [InlineData(OrganizationInvitationStatus.Cancelled)]
    [InlineData(OrganizationInvitationStatus.Expired)]
    public void TerminalInvitation_ShouldRejectFurtherTransitions(
        OrganizationInvitationStatus terminalStatus)
    {
        var invitation = CreateInvitation();
        var now = CreatedAt.AddDays(1);

        switch (terminalStatus)
        {
            case OrganizationInvitationStatus.Accepted:
                invitation.Accept(InvitedUserId, now);
                break;

            case OrganizationInvitationStatus.Declined:
                invitation.Decline(InvitedUserId, now);
                break;

            case OrganizationInvitationStatus.Cancelled:
                invitation.Cancel(now);
                break;

            case OrganizationInvitationStatus.Expired:
                invitation.Expire(invitation.ExpiresAt);
                break;
        }

        var later = invitation.ExpiresAt.AddDays(1);

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Accept(InvitedUserId, later));

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Decline(InvitedUserId, later));

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Cancel(later));

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Expire(later));
    }

    private static OrganizationInvitation CreateInvitation(
        DateTimeOffset? expiresAt = null)
    {
        return new OrganizationInvitation(
            OrganizationId.New(),
            InvitedUserId,
            UserId.New(),
            RoleId.New(),
            CreatedAt,
            expiresAt ?? CreatedAt.AddDays(7));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyIdentifiers()
    {
        Assert.Throws<ArgumentException>(() =>
            new OrganizationInvitation(
                new OrganizationId(Guid.Empty),
                InvitedUserId,
                UserId.New(),
                RoleId.New(),
                CreatedAt,
                CreatedAt.AddDays(7)));

        Assert.Throws<ArgumentException>(() =>
            new OrganizationInvitation(
                OrganizationId.New(),
                new UserId(Guid.Empty),
                UserId.New(),
                RoleId.New(),
                CreatedAt,
                CreatedAt.AddDays(7)));

        Assert.Throws<ArgumentException>(() =>
            new OrganizationInvitation(
                OrganizationId.New(),
                InvitedUserId,
                new UserId(Guid.Empty),
                RoleId.New(),
                CreatedAt,
                CreatedAt.AddDays(7)));

        Assert.Throws<ArgumentException>(() =>
            new OrganizationInvitation(
                OrganizationId.New(),
                InvitedUserId,
                UserId.New(),
                new RoleId(Guid.Empty),
                CreatedAt,
                CreatedAt.AddDays(7)));
    }

    [Fact]
    public void Cancel_ShouldRejectDateBeforeCreation()
    {
        var invitation = CreateInvitation();

        Assert.Throws<InvalidOperationException>(() =>
            invitation.Cancel(CreatedAt.AddTicks(-1)));

        Assert.Equal(OrganizationInvitationStatus.Pending, invitation.Status);
    }
}