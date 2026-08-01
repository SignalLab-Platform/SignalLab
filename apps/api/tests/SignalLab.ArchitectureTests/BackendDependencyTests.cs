namespace SignalLab.ArchitectureTests;

public sealed class BackendDependencyTests
{
    [Fact]
    public void Domain_ProjectReferences_ShouldBeEmpty()
    {
        AssertProjectReferences("SignalLab.Domain");
    }

    [Fact]
    public void Application_ProjectReferences_ShouldContainOnlyDomain()
    {
        AssertProjectReferences(
            "SignalLab.Application",
            "SignalLab.Domain");
    }

    [Fact]
    public void Infrastructure_ProjectReferences_ShouldContainApplicationAndDomain()
    {
        AssertProjectReferences(
            "SignalLab.Infrastructure",
            "SignalLab.Application",
            "SignalLab.Domain");
    }

    [Fact]
    public void Api_ProjectReferences_ShouldContainApplicationAndInfrastructure()
    {
        AssertProjectReferences(
            "SignalLab.Api",
            "SignalLab.Application",
            "SignalLab.Infrastructure");
    }

    [Fact]
    public void Domain_TechnologyReferences_ShouldBeEmpty()
    {
        var packageReferences =
            RepositoryArchitecture.ReadPackageReferences(
                "SignalLab.Domain");

        var frameworkReferences =
            RepositoryArchitecture.ReadFrameworkReferences(
                "SignalLab.Domain");

        Assert.Empty(packageReferences);
        Assert.Empty(frameworkReferences);
    }

    [Fact]
    public void Application_PresentationAndPersistenceReferences_ShouldBeAbsent()
    {
        var references = RepositoryArchitecture
            .ReadPackageReferences("SignalLab.Application")
            .Concat(
                RepositoryArchitecture.ReadFrameworkReferences(
                    "SignalLab.Application"))
            .ToArray();

        var forbiddenPrefixes = new[]
        {
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "Clerk",
        };

        foreach (var reference in references)
        {
            var isForbidden = forbiddenPrefixes.Any(
                prefix => reference.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase));

            Assert.False(
                isForbidden,
                $"SignalLab.Application must not reference '{reference}'.");
        }
    }

    private static void AssertProjectReferences(
        string projectName,
        params string[] expectedReferences)
    {
        var actualReferences = RepositoryArchitecture
            .ReadProjectReferences(projectName)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();

        var sortedExpectedReferences = expectedReferences
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();

        var referencesMatch = sortedExpectedReferences.SequenceEqual(
            actualReferences,
            StringComparer.Ordinal);

        Assert.True(
            referencesMatch,
            $"""
            Invalid project references for {projectName}.
            Expected: {FormatReferences(sortedExpectedReferences)}
            Actual:   {FormatReferences(actualReferences)}
            """);
    }

    private static string FormatReferences(
        IReadOnlyCollection<string> references)
    {
        return references.Count == 0
            ? "<none>"
            : string.Join(", ", references);
    }
}
