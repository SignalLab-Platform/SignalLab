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

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IExternalIdentityProvider>();

            services.AddScoped<IExternalIdentityProvider>(
                _ => new FakeExternalIdentityProvider(
                    new ExternalIdentity(
                        TestExternalUserId,
                        TestEmail),
                    () => ExternalIdentityProviderCallCount++));

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
            .Build();

        var connectionString = configuration
            .GetConnectionString("Postgres");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "User secret 'ConnectionStrings:Postgres' is missing or empty.");
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
        private readonly ExternalIdentity identity;
        private readonly Action onGetById;

        public FakeExternalIdentityProvider(
            ExternalIdentity identity,
            Action onGetById)
        {
            this.identity = identity;
            this.onGetById = onGetById;
        }

        public Task<ExternalIdentity?> GetByIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            onGetById();

            if (identity.Id != externalIdentityId)
            {
                return Task.FromResult<ExternalIdentity?>(null);
            }

            return Task.FromResult<ExternalIdentity?>(identity);
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

        await dbContext.Users
            .Where(user => user.ExternalIdentityId == TestExternalUserId)
            .ExecuteDeleteAsync();

        ExternalIdentityProviderCallCount = 0;
    }
}