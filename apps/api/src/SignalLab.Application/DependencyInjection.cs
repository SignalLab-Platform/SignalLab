using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Users;
using SignalLab.Application.Files;

namespace SignalLab.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ResolveCurrentUser>();
        services.AddScoped<GetCurrentUserProfile>();
        services.AddScoped<ChangeCurrentUserFirstName>();
        services.AddScoped<ChangeCurrentUserLastName>();
        services.AddScoped<SynchronizeCurrentUserEmail>();
        services.AddScoped<CreateAvatarUpload>();
        services.AddScoped<CompleteAvatarUpload>();
        services.AddScoped<GetStoredFileDownloadUrl>();

        return services;
    }
}