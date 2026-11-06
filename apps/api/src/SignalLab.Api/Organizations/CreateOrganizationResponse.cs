namespace SignalLab.Api.Organizations;

public sealed record CreateOrganizationResponse(
    Guid Id,
    string Name,
    string Country,
    string? Website);