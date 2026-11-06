namespace SignalLab.Api.Organizations;

public sealed record CreateOrganizationRequest(
    string? Name,
    string? Country,
    string? Website);