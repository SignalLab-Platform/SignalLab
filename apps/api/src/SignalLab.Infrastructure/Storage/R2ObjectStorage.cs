using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using SignalLab.Application.Storage;

namespace SignalLab.Infrastructure.Storage;

internal sealed class R2ObjectStorage : IObjectStorage
{
    private static readonly TimeSpan UploadUrlLifetime =
        TimeSpan.FromMinutes(10);

    private static readonly TimeSpan DownloadUrlLifetime =
        TimeSpan.FromHours(1);

    private readonly IAmazonS3 s3Client;
    private readonly ObjectStorageConfiguration configuration;

    public R2ObjectStorage(
        IAmazonS3 s3Client,
        ObjectStorageConfiguration configuration)
    {
        this.s3Client = s3Client;
        this.configuration = configuration;
    }

    public async Task<UploadUrl> CreateUploadUrlAsync(
        string objectKey,
        string contentType,
        CancellationToken cancellationToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(
            UploadUrlLifetime);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = configuration.BucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = expiresAt.UtcDateTime,
        };

        var url = await s3Client.GetPreSignedURLAsync(request);

        return new UploadUrl(
            new Uri(url),
            expiresAt);
    }

    public async Task<Uri> CreateDownloadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = configuration.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(DownloadUrlLifetime),
        };

        var url = await s3Client.GetPreSignedURLAsync(request);

        return new Uri(url);
    }

    public async Task<ObjectMetadata?> GetMetadataAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await s3Client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = configuration.BucketName,
                    Key = objectKey,
                },
                cancellationToken);

            return new ObjectMetadata(
                response.Headers.ContentType,
                response.ContentLength);
        }
        catch (AmazonS3Exception exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        await s3Client.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = configuration.BucketName,
                Key = objectKey,
            },
            cancellationToken);
    }
}