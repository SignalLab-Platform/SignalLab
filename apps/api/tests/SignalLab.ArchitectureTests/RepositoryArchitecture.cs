using System.Text.Json;
using System.Xml.Linq;

namespace SignalLab.ArchitectureTests;

internal static class RepositoryArchitecture
{
    private static readonly Lazy<string> RepositoryRoot =
        new(FindRepositoryRoot);

    public static IReadOnlyCollection<string> ReadProjectReferences(
        string projectName)
    {
        var document = LoadProject(projectName);

        return document
            .Descendants("ProjectReference")
            .Select(element => GetItemName(element, "Include"))
            .Where(reference => reference is not null)
            .Select(reference => reference!.Replace('\\', '/'))
            .Select(reference =>
                Path.GetFileNameWithoutExtension(reference) ??
                throw new InvalidOperationException(
                    $"Unable to extract the project name from reference '{reference}'."))
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyCollection<string> ReadPackageReferences(
        string projectName)
    {
        var document = LoadProject(projectName);

        return document
            .Descendants("PackageReference")
            .Select(element =>
                GetItemName(element, "Include") ??
                GetItemName(element, "Update"))
            .Where(reference => reference is not null)
            .Select(reference => reference!)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyCollection<string> ReadFrameworkReferences(
        string projectName)
    {
        var document = LoadProject(projectName);

        return document
            .Descendants("FrameworkReference")
            .Select(element => GetItemName(element, "Include"))
            .Where(reference => reference is not null)
            .Select(reference => reference!)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlySet<string> ReadFrontendDependencies()
    {
        var packageJsonPath = Path.Combine(
            RepositoryRoot.Value,
            "apps",
            "web",
            "package.json");

        using var document = JsonDocument.Parse(
            File.ReadAllText(packageJsonPath));

        var dependencies = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        var sectionNames = new[]
        {
            "dependencies",
            "devDependencies",
            "peerDependencies",
            "optionalDependencies",
        };

        foreach (var sectionName in sectionNames)
        {
            if (!document.RootElement.TryGetProperty(
                    sectionName,
                    out var section))
            {
                continue;
            }

            foreach (var dependency in section.EnumerateObject())
            {
                dependencies.Add(dependency.Name);
            }
        }

        return dependencies;
    }

    private static XDocument LoadProject(string projectName)
    {
        return XDocument.Load(GetProjectPath(projectName));
    }

    private static string GetProjectPath(string projectName)
    {
        var relativePath = projectName switch
        {
            "SignalLab.Api" =>
                "apps/api/src/SignalLab.Api/SignalLab.Api.csproj",

            "SignalLab.Application" =>
                "apps/api/src/SignalLab.Application/SignalLab.Application.csproj",

            "SignalLab.Domain" =>
                "apps/api/src/SignalLab.Domain/SignalLab.Domain.csproj",

            "SignalLab.Infrastructure" =>
                "apps/api/src/SignalLab.Infrastructure/SignalLab.Infrastructure.csproj",

            _ => throw new ArgumentOutOfRangeException(
                nameof(projectName),
                projectName,
                "Unknown SignalLab project."),
        };

        return Path.Combine(
            RepositoryRoot.Value,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string? GetItemName(
        XElement element,
        string attributeName)
    {
        return element.Attribute(attributeName)?.Value;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            var globalJsonPath = Path.Combine(
                currentDirectory.FullName,
                "global.json");

            var solutionPath = Path.Combine(
                currentDirectory.FullName,
                "apps",
                "api",
                "SignalLab.Api.sln");

            var frontendPackagePath = Path.Combine(
                currentDirectory.FullName,
                "apps",
                "web",
                "package.json");

            if (File.Exists(globalJsonPath) &&
                File.Exists(solutionPath) &&
                File.Exists(frontendPackagePath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            "The SignalLab repository root could not be found.");
    }
}
