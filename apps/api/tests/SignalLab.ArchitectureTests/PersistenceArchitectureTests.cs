using Microsoft.EntityFrameworkCore;
using SignalLab.Infrastructure.Persistence;

namespace SignalLab.ArchitectureTests;

public sealed class PersistenceArchitectureTests
{
    [Fact]
    public void EntityConfigurations_ShouldBeUniquePerEntityType()
    {
        var configurationTypes = GetEntityConfigurations();

        var duplicatedEntityTypes = configurationTypes
            .GroupBy(configuration => configuration.EntityType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            duplicatedEntityTypes.Length == 0,
            $"""
            Multiple IEntityTypeConfiguration implementations target the same entity.
            Entities: {string.Join(", ", duplicatedEntityTypes)}
            """);
    }

    [Fact]
    public void EntityConfigurations_ShouldUseEntityConfigurationNaming()
    {
        var invalidConfigurations = GetEntityConfigurations()
            .Where(configuration =>
                configuration.ConfigurationType.Name !=
                $"{configuration.EntityType.Name}Configuration")
            .Select(configuration => configuration.ConfigurationType.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            invalidConfigurations.Length == 0,
            $"""
            Entity configurations must be named <EntityName>Configuration.
            Invalid configurations: {string.Join(", ", invalidConfigurations)}
            """);
    }

    [Fact]
    public void Infrastructure_PersistencePackages_ShouldBeConfigured()
    {
        var packageReferences = RepositoryArchitecture
            .ReadPackageReferences("SignalLab.Infrastructure");

        var requiredPackages = new[]
        {
            "Microsoft.EntityFrameworkCore.Design",
            "Microsoft.EntityFrameworkCore.Relational",
            "Npgsql.EntityFrameworkCore.PostgreSQL",
        };

        foreach (var requiredPackage in requiredPackages)
        {
            Assert.Contains(
                requiredPackage,
                packageReferences,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    private static IReadOnlyCollection<EntityConfiguration> GetEntityConfigurations()
    {
        var configurationInterface = typeof(IEntityTypeConfiguration<>);

        return typeof(SignalLabDbContext)
            .Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(configurationType => configurationType
                .GetInterfaces()
                .Where(type =>
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == configurationInterface)
                .Select(type => new EntityConfiguration(
                    configurationType,
                    type.GetGenericArguments()[0])))
            .ToArray();
    }

    private sealed record EntityConfiguration(
        Type ConfigurationType,
        Type EntityType);
}