using Microsoft.Extensions.DependencyInjection;
using SignalLab.Application.Users;
using SignalLab.Application.Files;
using SignalLab.Application.Organizations;
using SignalLab.Application.Notifications;

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
        services.AddScoped<CreateOrganization>();
        services.AddScoped<GetCurrentUserOrganizations>();
        services.AddScoped<OrganizationAccessService>();
        services.AddScoped<GetOrganization>();
        services.AddScoped<GetOrganizationMembers>();
        services.AddScoped<GetOrganizationRoles>();
        services.AddScoped<CreateOrganizationInvitation>();
        services.AddScoped<GetOrganizationInvitations>();
        services.AddScoped<GetOrganizationInvitation>();
        services.AddScoped<GetCurrentUserInvitations>();
        services.AddScoped<AcceptOrganizationInvitation>();
        services.AddScoped<DeclineOrganizationInvitation>();
        services.AddScoped<CancelOrganizationInvitation>();
        services.AddScoped<GetCurrentUserNotifications>();
        services.AddScoped<CountCurrentUserUnreadNotifications>();
        services.AddScoped<MarkNotificationAsRead>();

        return services;
    }
}