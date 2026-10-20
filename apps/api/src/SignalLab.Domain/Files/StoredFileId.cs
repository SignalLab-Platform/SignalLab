namespace SignalLab.Domain.Files;

public readonly record struct StoredFileId(Guid Value)
{
    public static StoredFileId New()
    {
        return new StoredFileId(Guid.NewGuid());
    }
}