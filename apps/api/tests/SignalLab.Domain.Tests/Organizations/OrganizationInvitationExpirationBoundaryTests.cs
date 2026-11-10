using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Domain.Tests.Organizations;

public sealed class OrganizationInvitationExpirationBoundaryTests
{
    [Fact]
    public void BeforeExpiration_ShouldRemainPending()
    {
        var invitation = CreateInvitation();

        var now = invitation.ExpiresAt.AddTicks(-1);

        Assert.False(invitation.IsExpired(now));

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitation.GetEffectiveStatus(now));

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitation.Status);
    }

    [Fact]
    public void AtExactExpiration_ShouldBeExpired()
    {
        var invitation = CreateInvitation();

        var now = invitation.ExpiresAt;

        Assert.True(invitation.IsExpired(now));

        Assert.Equal(
            OrganizationInvitationStatus.Expired,
            invitation.GetEffectiveStatus(now));

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitation.Status);
    }

    [Fact]
    public void AfterExpiration_ShouldBeExpired()
    {
        var invitation = CreateInvitation();

        var now = invitation.ExpiresAt.AddTicks(1);

        Assert.True(invitation.IsExpired(now));

        Assert.Equal(
            OrganizationInvitationStatus.Expired,
            invitation.GetEffectiveStatus(now));

        Assert.Equal(
            OrganizationInvitationStatus.Pending,
            invitation.Status);
    }

    private static OrganizationInvitation CreateInvitation()
    {
        var createdAt = UtcNow();

        return new OrganizationInvitation(
            OrganizationId.New(),
            UserId.New(),
            UserId.New(),
            RoleId.New(),
            createdAt,
            createdAt.AddDays(7));
    }

    private static DateTimeOffset UtcNow()
    {
        var now = DateTimeOffset.UtcNow;

        return new DateTimeOffset(
            now.Ticks - now.Ticks % 10,
            TimeSpan.Zero);
    }
}