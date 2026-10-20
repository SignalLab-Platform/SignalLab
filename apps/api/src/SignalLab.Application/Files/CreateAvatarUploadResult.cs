using SignalLab.Domain.Files;

namespace SignalLab.Application.Files;

public sealed record CreateAvatarUploadResult(
    StoredFileId StoredFileId,
    Uri UploadUrl,
    DateTimeOffset ExpiresAt);