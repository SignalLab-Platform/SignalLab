namespace SignalLab.Api.Organizations;

public sealed record OrganizationDetailsResponse(
    Guid Id,
    string Name,
    string Country,
    string? Website,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);