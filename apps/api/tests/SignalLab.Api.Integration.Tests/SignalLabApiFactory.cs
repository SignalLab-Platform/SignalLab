using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SignalLab.Application.Identity;
using SignalLab.Application.Storage;
using SignalLab.Domain.Users;
using SignalLab.Domain.Files;
using SignalLab.Api.Integration.Tests.Infrastructure;

namespace SignalLab.Api.Integration.Tests;

public sealed class SignalLabApiFactory : WebApplicationFactory<Program>
{
    public TestDatabase Database => new(Services);

    public const string TestIssuer = "https://clerk.example.test";
    public const string TestAuthorizedParty = "https://web.example.test";
    public const string TestAllowedOrigin = "https://web.example.test";
    public const string TestExternalUserId = "user_test_123";
    public const string TestEmail = "user@example.com";

    private readonly int invitationUserLimit;
    private readonly int invitationOrganizationLimit;

    public SignalLabApiFactory()
        : this(10000, 10000)
    {
    }

    internal SignalLabApiFactory(
        int invitationUserLimit,
        int invitationOrganizationLimit)
    {
        this.invitationUserLimit = invitationUserLimit;
        this.invitationOrganizationLimit = invitationOrganizationLimit;
    }

    internal TestJwtTokenFactory TokenFactory { get; } = new();

    public int ExternalIdentityProviderCallCount { get; private set; }

    public string ExternalIdentityEmail { get; set; } = TestEmail;

    public string ExternalIdentityUserId { get; set; } = TestExternalUserId;

    public ObjectMetadata? ObjectStorageMetadata { get; set; } =
        new ObjectMetadata(
            "image/png",
            1024);

    public string? LastObjectStorageMetadataKey { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting(
            "OrganizationInvitations:RateLimiting:UserPermitLimit",
            invitationUserLimit.ToString());

        builder.UseSetting(
            "OrganizationInvitations:RateLimiting:OrganizationPermitLimit",
            invitationOrganizationLimit.ToString());

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
                _ => new TestExternalIdentityProvider(
                    () => new ExternalIdentity(
                        ExternalIdentityUserId,
                        ExternalIdentityEmail),
                    () => ExternalIdentityProviderCallCount++));

            services.RemoveAll<IObjectStorage>();

            services.AddScoped<IObjectStorage>(
                _ => new TestObjectStorage(
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

    public Task InitializeDatabaseAsync()
    {
        return Database.InitializeAsync();
    }

    public async Task ResetTestUserAsync()
    {
        await Database.ResetUserAsync(TestExternalUserId);
        await Database.ResetUserAsync("user_test_456");

        ExternalIdentityProviderCallCount = 0;
        ExternalIdentityUserId = TestExternalUserId;
        ExternalIdentityEmail = TestEmail;

        ObjectStorageMetadata = new ObjectMetadata(
            "image/png",
            1024);

        LastObjectStorageMetadataKey = null;
    }

    public Task<User?> GetTestUserAsync()
    {
        return Database.GetUserAsync(TestExternalUserId);
    }

    public Task<StoredFile?> GetStoredFileAsync(
        StoredFileId storedFileId)
    {
        return Database.GetStoredFileAsync(storedFileId);
    }
}