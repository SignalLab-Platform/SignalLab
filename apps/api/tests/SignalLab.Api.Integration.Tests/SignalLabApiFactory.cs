using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalLab.Infrastructure.Persistence;
using SignalLab.Application.Identity;
using SignalLab.Application.Storage;
using SignalLab.Domain.Users;
using SignalLab.Domain.Files;

namespace SignalLab.Api.Integration.Tests;

public sealed class SignalLabApiFactory : WebApplicationFactory<Program>
{
    public const string TestIssuer = "https://clerk.example.test";
    public const string TestAuthorizedParty = "https://web.example.test";
    public const string TestAllowedOrigin = "https://web.example.test";
    public const string TestExternalUserId = "user_test_123";
    public const string TestEmail = "user@example.com";

    internal TestJwtTokenFactory TokenFactory { get; } = new();

    public int ExternalIdentityProviderCallCount { get; private set; }

    public string ExternalIdentityEmail { get; set; } = TestEmail;

    public ObjectMetadata? ObjectStorageMetadata { get; set; } =
        new ObjectMetadata(
            "image/png",
            1024);

    public string? LastObjectStorageMetadataKey { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting(
            "ConnectionStrings:Postgres",
            CreateTestPostgresConnectionString());

        builder.UseSetting(
            "Authentication:Clerk:Issuer",
            TestIssuer);

        builder.UseSetting(
            "Authentication:Clerk:AuthorizedParties:0",
            TestAuthorizedParty);

        builder.UseSetting(
            "Authentication:Clerk:SecretKey",
            "sk_test_not-a-real-secret");

        builder.UseSetting(
            "Cors:AllowedOrigins:0",
            TestAllowedOrigin);

        builder.UseSetting(
            "ObjectStorage:Endpoint",
            "https://r2.example.test");

        builder.UseSetting(
            "ObjectStorage:BucketName",
            "signallab-tests");

        builder.UseSetting(
            "ObjectStorage:AccessKeyId",
            "test-access-key");

        builder.UseSetting(
            "ObjectStorage:SecretAccessKey",
            "test-secret-key");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IExternalIdentityProvider>();

            services.AddScoped<IExternalIdentityProvider>(
                _ => new FakeExternalIdentityProvider(
                    () => new ExternalIdentity(
                        TestExternalUserId,
                        ExternalIdentityEmail),
                    () => ExternalIdentityProviderCallCount++));

            services.RemoveAll<IObjectStorage>();

            services.AddScoped<IObjectStorage>(
                _ => new FakeObjectStorage(
                    () => ObjectStorageMetadata,
                    objectKey => LastObjectStorageMetadataKey = objectKey));

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Authority = null;
                    options.ConfigurationManager = null;

                    options.TokenValidationParameters.IssuerSigningKey =
                        TokenFactory.ValidationKey;
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TokenFactory.Dispose();
        }

        base.Dispose(disposing);
    }

    private static string CreateTestPostgresConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration
            .GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configuration 'ConnectionStrings:Postgres' is missing or empty.");
        }

        var connectionStringBuilder =
            new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "signallab_tests"
            };

        return connectionStringBuilder.ConnectionString;
    }

    private sealed class FakeExternalIdentityProvider : IExternalIdentityProvider
    {
        private readonly Func<ExternalIdentity> getIdentity;
        private readonly Action onGetById;

        public FakeExternalIdentityProvider(
            Func<ExternalIdentity> getIdentity,
            Action onGetById)
        {
            this.getIdentity = getIdentity;
            this.onGetById = onGetById;
        }

        public Task<ExternalIdentity?> GetByIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            onGetById();

            var identity = getIdentity();

            if (identity.Id != externalIdentityId)
            {
                return Task.FromResult<ExternalIdentity?>(null);
            }

            return Task.FromResult<ExternalIdentity?>(identity);
        }
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        private readonly Func<ObjectMetadata?> getMetadata;
        private readonly Action<string> onGetMetadata;

        public FakeObjectStorage(
            Func<ObjectMetadata?> getMetadata,
            Action<string> onGetMetadata)
        {
            this.getMetadata = getMetadata;
            this.onGetMetadata = onGetMetadata;
        }

        public Task<UploadUrl> CreateUploadUrlAsync(
            string objectKey,
            string contentType,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new UploadUrl(
                    new Uri("https://storage.example.test/upload"),
                    DateTimeOffset.UtcNow.AddMinutes(10)));
        }

        public Task<Uri> CreateDownloadUrlAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new Uri("https://storage.example.test/download"));
        }

        public Task<ObjectMetadata?> GetMetadataAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            onGetMetadata(objectKey);

            return Task.FromResult(getMetadata());
        }

        public Task DeleteAsync(
            string objectKey,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task ResetTestUserAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        var userId = await dbContext.Users
            .Where(user =>
                user.ExternalIdentityId == TestExternalUserId)
            .Select(user => (UserId?)user.Id)
            .SingleOrDefaultAsync();

        if (userId is not null)
        {
            await dbContext.StoredFiles
                .Where(file =>
                    file.OwnerUserId == userId.Value)
                .ExecuteDeleteAsync();

            await dbContext.Users
                .Where(user =>
                    user.Id == userId.Value)
                .ExecuteDeleteAsync();
        }

        ExternalIdentityProviderCallCount = 0;
        ExternalIdentityEmail = TestEmail;

        ObjectStorageMetadata = new ObjectMetadata(
            "image/png",
            1024);

        LastObjectStorageMetadataKey = null;
    }

    public async Task<User?> GetTestUserAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        return await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.ExternalIdentityId == TestExternalUserId);
    }

    public async Task<StoredFile?> GetStoredFileAsync(
        StoredFileId storedFileId)
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<SignalLabDbContext>();

        return await dbContext.StoredFiles
            .AsNoTracking()
            .SingleOrDefaultAsync(
                storedFile => storedFile.Id == storedFileId);
    }
}