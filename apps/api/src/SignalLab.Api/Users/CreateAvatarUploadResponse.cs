namespace SignalLab.Api.Users;

public sealed record CreateAvatarUploadResponse(
    Guid FileId,
    string UploadUrl,
    DateTimeOffset ExpiresAt);