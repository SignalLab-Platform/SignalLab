namespace SignalLab.Api.Organizations;

public sealed record OrganizationSummaryResponse(
    Guid Id,
    string Name,
    string Country,
    string? Website);