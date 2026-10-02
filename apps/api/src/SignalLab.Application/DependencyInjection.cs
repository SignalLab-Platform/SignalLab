using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Users;

namespace SignalLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ResolveCurrentUser>();

        return services;
    }
}