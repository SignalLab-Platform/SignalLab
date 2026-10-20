namespace SignalLab.Api.Users;

public sealed record CurrentUserProfileResponse(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    string Language,
    Guid? AvatarFileId,
    string? AvatarUrl);