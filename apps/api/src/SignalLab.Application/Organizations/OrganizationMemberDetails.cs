using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public sealed record OrganizationMemberDetails(
    OrganizationMemberId Id,
    UserId UserId,
    string? FirstName,
    string? LastName,
    string Email,
    RoleId RoleId,
    string RoleName,
    OrganizationMemberStatus Status,
    DateTimeOffset JoinedAt,
    DateTimeOffset? RemovedAt);