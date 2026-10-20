using Microsoft.Extensions.Configuration;

namespace SignalLab.Infrastructure.Storage;

internal sealed record ObjectStorageConfiguration(
    string Endpoint,
    string BucketName,
    string AccessKeyId,
    string SecretAccessKey)
{
    public static ObjectStorageConfiguration FromConfiguration(
        IConfiguration configuration)
    {
        return new ObjectStorageConfiguration(
            GetRequiredValue(
                configuration,
                "ObjectStorage:Endpoint"),
            GetRequiredValue(
                configuration,
                "ObjectStorage:BucketName"),
            GetRequiredValue(
                configuration,
                "ObjectStorage:AccessKeyId"),
            GetRequiredValue(
                configuration,
                "ObjectStorage:SecretAccessKey"));
    }

    private static string GetRequiredValue(
        IConfiguration configuration,
        string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required configuration '{key}' is missing or empty.");
        }

        return value;
    }
}