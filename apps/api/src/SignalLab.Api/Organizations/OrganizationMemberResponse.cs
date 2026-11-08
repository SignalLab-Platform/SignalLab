namespace SignalLab.Api.Organizations;

public sealed record OrganizationMemberResponse(
    Guid Id,
    Guid UserId,
    string? FirstName,
    string? LastName,
    string Email,
    Guid RoleId,
    string RoleName,
    string Status,
    DateTimeOffset JoinedAt,
    DateTimeOffset? RemovedAt);