using SignalLab.Domain.Organizations;

namespace SignalLab.Application.Organizations;

public enum OrganizationPermissionStatus
{
    Granted,
    OrganizationNotFound,
    PermissionDenied
}

public sealed record OrganizationPermissionResult(
    OrganizationPermissionStatus Status,
    Organization? Organization);