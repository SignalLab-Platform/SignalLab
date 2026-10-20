namespace SignalLab.Api.Users;

public sealed record CreateAvatarUploadRequest(
    string FileName,
    string ContentType,
    long Size);