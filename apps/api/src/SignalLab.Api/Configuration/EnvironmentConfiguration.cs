namespace SignalLab.Api.Configuration;

public static class EnvironmentConfiguration
{
    private static readonly HashSet<string> SupportedEnvironments =
        new(StringComparer.Ordinal)
        {
            Environments.Development,
            Environments.Staging,
            Environments.Production,
        };

    public static void Validate(IHostEnvironment environment)
    {
        if (SupportedEnvironments.Contains(environment.EnvironmentName))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Unsupported environment '{environment.EnvironmentName}'. " +
            $"Supported environments are: {string.Join(", ", SupportedEnvironments)}.");
    }
}