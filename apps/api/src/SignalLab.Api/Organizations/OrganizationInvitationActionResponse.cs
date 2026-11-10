using SignalLab.Domain.Organizations;

namespace SignalLab.Api.Organizations;

public sealed record OrganizationInvitationActionResponse(
    Guid Id,
    Guid OrganizationId,
    Guid InvitedUserId,
    Guid InvitedByUserId,
    Guid RoleId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RespondedAt)
{
    public static OrganizationInvitationActionResponse From(
        OrganizationInvitation invitation)
    {
        return new OrganizationInvitationActionResponse(
            invitation.Id.Value,
            invitation.OrganizationId.Value,
            invitation.InvitedUserId.Value,
            invitation.InvitedByUserId.Value,
            invitation.RoleId.Value,
            invitation.Status.ToString(),
            invitation.CreatedAt,
            invitation.UpdatedAt,
            invitation.ExpiresAt,
            invitation.RespondedAt);
    }
}