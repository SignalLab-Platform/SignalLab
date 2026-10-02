using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SignalLab.Api.Authentication;
using SignalLab.Api.Configuration;
using SignalLab.Infrastructure;

namespace SignalLab.Api.Integration.Tests;

public sealed class ConfigurationValidationTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void EnvironmentConfiguration_SupportedEnvironment_ShouldSucceed(string environmentName)
    {
        var environment = new TestHostEnvironment(environmentName);

        EnvironmentConfiguration.Validate(environment);
    }

    [Fact]
    public void EnvironmentConfiguration_UnsupportedEnvironment_ShouldFail()
    {
        var environment = new TestHostEnvironment("Banana");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            EnvironmentConfiguration.Validate(environment));

        Assert.Contains("Unsupported environment 'Banana'.", exception.Message);
        Assert.Contains(Environments.Development, exception.Message);
        Assert.Contains(Environments.Staging, exception.Message);
        Assert.Contains(Environments.Production, exception.Message);
    }

    [Fact]
    public void Infrastructure_MissingPostgresConnectionString_ShouldFail()
    {
        var configuration = CreateConfiguration();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Equal(
            "Required configuration 'ConnectionStrings:Postgres' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Infrastructure_EmptyPostgresConnectionString_ShouldFail()
    {
        var configuration = CreateConfiguration(" ");
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Equal(
            "Required configuration 'ConnectionStrings:Postgres' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Infrastructure_InvalidPostgresConnectionString_ShouldFail()
    {
        var configuration = CreateConfiguration("this-is-not-a-connection-string");
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Equal(
            "Configuration 'ConnectionStrings:Postgres' is not a valid PostgreSQL connection string.",
            exception.Message);
    }

    [Theory]
    [InlineData(
        "Database=signallab;Username=signallab;Password=test",
        "Configuration 'ConnectionStrings:Postgres' must define a Host.")]
    [InlineData(
        "Host=localhost;Username=signallab;Password=test",
        "Configuration 'ConnectionStrings:Postgres' must define a Database.")]
    [InlineData(
        "Host=localhost;Database=signallab;Password=test",
        "Configuration 'ConnectionStrings:Postgres' must define a Username.")]
    public void Infrastructure_IncompletePostgresConnectionString_ShouldFail(
        string connectionString,
        string expectedMessage)
    {
        var configuration = CreateConfiguration(connectionString);
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Infrastructure_ValidPostgresConnectionString_ShouldSucceed()
    {
        var configuration = CreateConfiguration(
            postgresConnectionString:
                "Host=localhost;Port=5432;Database=signallab;Username=signallab;Password=test",
            clerkSecretKey:
                "sk_test_not-a-real-secret");

        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);
    }

    [Fact]
    public void Infrastructure_MissingClerkSecretKey_ShouldFail()
    {
        var configuration = CreateConfiguration(
            postgresConnectionString:
                "Host=localhost;Port=5432;Database=signallab;Username=signallab;Password=test");

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddInfrastructure(configuration));

        Assert.Equal(
            "Required configuration 'Authentication:Clerk:SecretKey' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Authentication_MissingIssuer_ShouldFail()
    {
        var configuration = CreateConfiguration();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabAuthentication(configuration));

        Assert.Equal(
            "Required configuration 'Authentication:Clerk:Issuer' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Authentication_MissingAuthorizedParties_ShouldFail()
    {
        var configuration = CreateConfiguration(clerkIssuer: "https://clerk.example.test");
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabAuthentication(configuration));

        Assert.Equal(
            "Required configuration 'Authentication:Clerk:AuthorizedParties' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Authentication_ValidConfiguration_ShouldSucceed()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: "https://clerk.example.test",
            clerkAuthorizedParty: "https://web.example.test");

        var services = new ServiceCollection();

        services.AddSignalLabAuthentication(configuration);
    }

    [Fact]
    public void Authentication_EmptyIssuer_ShouldFail()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: " ",
            clerkAuthorizedParty: "https://web.example.test");

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabAuthentication(configuration));

        Assert.Equal(
            "Required configuration 'Authentication:Clerk:Issuer' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Authentication_WhitespaceAuthorizedParty_ShouldFail()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: "https://clerk.example.test",
            clerkAuthorizedParty: " ");

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabAuthentication(configuration));

        Assert.Equal(
            "Required configuration 'Authentication:Clerk:AuthorizedParties' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Cors_MissingAllowedOrigins_ShouldFail()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: "https://clerk.example.test",
            clerkAuthorizedParty: "https://web.example.test");

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabCors(configuration));

        Assert.Equal(
            "Required configuration 'Cors:AllowedOrigins' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Cors_EmptyAllowedOrigins_ShouldFail()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: "https://clerk.example.test",
            clerkAuthorizedParty: "https://web.example.test",
            corsAllowedOrigin: " ");

        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSignalLabCors(configuration));

        Assert.Equal(
            "Required configuration 'Cors:AllowedOrigins' is missing or empty.",
            exception.Message);
    }

    [Fact]
    public void Cors_ValidAllowedOrigins_ShouldSucceed()
    {
        var configuration = CreateConfiguration(
            clerkIssuer: "https://clerk.example.test",
            clerkAuthorizedParty: "https://web.example.test",
            corsAllowedOrigin: "https://web.example.test");

        var services = new ServiceCollection();

        services.AddSignalLabCors(configuration);
    }

    private static IConfiguration CreateConfiguration(
        string? postgresConnectionString = null,
        string? clerkIssuer = null,
        string? clerkAuthorizedParty = null,
        string? clerkSecretKey = null,
        string? corsAllowedOrigin = null)
    {
        var values = new Dictionary<string, string?>();

        if (postgresConnectionString is not null)
        {
            values["ConnectionStrings:Postgres"] = postgresConnectionString;
        }

        if (clerkIssuer is not null)
        {
            values["Authentication:Clerk:Issuer"] = clerkIssuer;
        }

        if (clerkAuthorizedParty is not null)
        {
            values["Authentication:Clerk:AuthorizedParties:0"] =
                clerkAuthorizedParty;
        }

        if (clerkSecretKey is not null)
        {
            values["Authentication:Clerk:SecretKey"] = clerkSecretKey;
        }

        if (corsAllowedOrigin is not null)
        {
            values["Cors:AllowedOrigins:0"] = corsAllowedOrigin;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "SignalLab.Api.IntegrationTests";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}