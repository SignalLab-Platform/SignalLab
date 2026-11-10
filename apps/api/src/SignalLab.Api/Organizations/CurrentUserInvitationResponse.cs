using SignalLab.Application.Organizations;

namespace SignalLab.Api.Organizations;

public sealed record CurrentUserInvitationResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid InvitedByUserId,
    Guid RoleId,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RespondedAt)
{
    public static CurrentUserInvitationResponse From(
        CurrentUserInvitationDetails details)
    {
        return new CurrentUserInvitationResponse(
            details.Id.Value,
            details.OrganizationId.Value,
            details.OrganizationName,
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