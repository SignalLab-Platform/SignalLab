namespace SignalLab.Api.Organizations;

public sealed record OrganizationRoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsOwner,
    IReadOnlyList<string> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);