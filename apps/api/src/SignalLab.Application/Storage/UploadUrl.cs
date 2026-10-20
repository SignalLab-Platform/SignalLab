namespace SignalLab.Application.Storage;

public sealed record UploadUrl(
    Uri Url,
    DateTimeOffset ExpiresAt);