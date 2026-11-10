using SignalLab.Application.Organizations;

namespace SignalLab.Api.Organizations;

public sealed record OrganizationInvitationResponse(
    Guid Id,
    Guid OrganizationId,
    Guid InvitedUserId,
    string? InvitedUserFirstName,
    string? InvitedUserLastName,
    string InvitedUserEmail,
    Guid InvitedByUserId,
    Guid RoleId,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RespondedAt)
{
    public static OrganizationInvitationResponse From(
        OrganizationInvitationDetails details)
    {
        return new OrganizationInvitationResponse(
            details.Id.Value,
            details.OrganizationId.Value,
            details.InvitedUserId.Value,
            details.InvitedUserFirstName,
            details.InvitedUserLastName,
            details.InvitedUserEmail,
            details.InvitedByUserId.Value,
            details.RoleId.Value,
            details.RoleName,
            details.Status.ToString(),
            details.CreatedAt,
            details.UpdatedAt,
            details.ExpiresAt,
            details.RespondedAt);
    }
}