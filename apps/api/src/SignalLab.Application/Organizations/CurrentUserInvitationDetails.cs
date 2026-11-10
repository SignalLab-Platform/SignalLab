using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed record CurrentUserInvitationDetails(
    OrganizationInvitationId Id,
    OrganizationId OrganizationId,
    string OrganizationName,
    UserId InvitedByUserId,
    RoleId RoleId,
    string RoleName,
    OrganizationInvitationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RespondedAt);