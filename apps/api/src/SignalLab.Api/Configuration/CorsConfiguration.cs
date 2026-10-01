namespace SignalLab.Api.Configuration;

public static class CorsConfiguration
{
    public const string PolicyName = "SignalLabWeb";

    public static IServiceCollection AddSignalLabCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (allowedOrigins.Length == 0 ||
            allowedOrigins.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "Required configuration 'Cors:AllowedOrigins' is missing or empty.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}