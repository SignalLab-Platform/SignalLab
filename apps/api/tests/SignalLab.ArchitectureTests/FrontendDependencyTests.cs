namespace SignalLab.ArchitectureTests;

public sealed class FrontendDependencyTests
{
    [Fact]
    public void Frontend_CanonicalPackages_ShouldBePresent()
    {
        var dependencies =
            RepositoryArchitecture.ReadFrontendDependencies();

        Assert.Contains("next", dependencies);
        Assert.Contains("@tanstack/react-query", dependencies);
    }

    [Fact]
    public void Frontend_CompetingRoutersAndVite_ShouldBeAbsent()
    {
        var dependencies =
            RepositoryArchitecture.ReadFrontendDependencies();

        var forbiddenDependencies = new[]
        {
            "vite",
            "@vitejs/plugin-react",
            "@tanstack/react-router",
            "react-router",
            "react-router-dom",
        };

        foreach (var forbiddenDependency in forbiddenDependencies)
        {
            Assert.DoesNotContain(
                forbiddenDependency,
                dependencies);
        }
    }
}
