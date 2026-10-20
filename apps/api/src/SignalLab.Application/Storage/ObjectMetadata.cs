namespace SignalLab.Application.Storage;

public sealed record ObjectMetadata(
    string ContentType,
    long Size);