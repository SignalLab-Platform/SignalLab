using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed record OrganizationInvitationDetails(
    OrganizationInvitationId Id,
    OrganizationId OrganizationId,
    UserId InvitedUserId,
    string? InvitedUserFirstName,
    string? InvitedUserLastName,
    string InvitedUserEmail,
    UserId InvitedByUserId,
    RoleId RoleId,
    string RoleName,
    OrganizationInvitationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RespondedAt)
{
    public static OrganizationInvitationDetails From(
        OrganizationInvitation invitation,
        Organization organization,
        User invitedUser,
        DateTimeOffset now)
    {
        var role = organization.Roles.Single(
            role => role.Id == invitation.RoleId);

        return new OrganizationInvitationDetails(
            invitation.Id,
            invitation.OrganizationId,
            invitation.InvitedUserId,
            invitedUser.FirstName,
            invitedUser.LastName,
            invitedUser.Email,
            invitation.InvitedByUserId,
            invitation.RoleId,
            role.Name,
            invitation.GetEffectiveStatus(now),
            invitation.CreatedAt,
            invitation.UpdatedAt,
            invitation.ExpiresAt,
            invitation.RespondedAt);
    }
}