namespace SignalLab.Api.Organizations;

public sealed record CreateOrganizationInvitationRequest(
    string? Email,
    Guid RoleId,
    int Duration);