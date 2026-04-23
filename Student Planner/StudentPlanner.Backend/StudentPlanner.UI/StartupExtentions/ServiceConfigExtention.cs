using MailKit.Net.Smtp;
using StudentPlanner.Core;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.AcademicEvents.ServiceContracts;
using StudentPlanner.Core.Application.AcademicEvents.Services;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.EventRequests;
using StudentPlanner.Core.Application.EventRequests.Strategies;
using StudentPlanner.Core.Application.Events.EventPreveiws;
using StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;
using StudentPlanner.Core.Application.Events.UsosEvents.Services;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.Core.Application.PersonalEvents;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.Core.Application.Notifications.Services;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Infrastructure.Identity;
using StudentPlanner.Infrastructure.Repositories;
using StudentPlanner.Infrastructure.Repositories.Events;
using StudentPlanner.Infrastructure.Services;
using StudentPlanner.Infrastructure.Services.Settings;
using StudentPlanner.UI.NotificationServices;
namespace StudentPlanner.Backend;

/// <summary>
/// Extension methods for application service registration in the DI container.
/// </summary>
public static class ServiceConfigExtention
{
    /// <summary>
    /// Configures application services, repositories, and third-party integrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration.</param>
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        //mail
        services.AddTransient<ISmtpClient, SmtpClient>();

        //config
        services.Configure<UsosApiSettings>(config.GetSection("UsosApi"));

        //services
        services.AddScoped<IPersonalEventService, PersonalEventService>();
        services.AddScoped<IEventRequestService, EventRequestService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAcademicEventService, AcademicEventService>();
        services.AddTransient<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, MailtrapEmailService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IUsosEventService, UsosEventService>();
        services.AddScoped<IUsosEventRepository, UsosEventRepository>();
        services.AddScoped<IEventPreviewService, EventPreviewService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();

        //notification services
        services.AddScoped<IEventRequestNotificationService, EventRequestNotificationService>();

        //clients
        services.AddHttpClient<IUsosClient, UsosClient>(client =>
        {
            client.BaseAddress = new Uri(config["UsosApi:BaseUrl"]!);
        });

        //repo
        services.AddScoped<IPersonalEventRepository, PersonalEventRepository>();
        services.AddScoped<IEventRequestRepository, EventRequestRepository>();
        services.AddScoped<IAcademicEventRepository, AcademicEventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();

        services.AddTransient<IEventRequestApprovalStrategy, StudentPlanner.Core.Application.EventRequests.Strategies.CreateApprovalStrategy>();
        services.AddTransient<IEventRequestApprovalStrategy, StudentPlanner.Core.Application.EventRequests.Strategies.UpdateApprovalStrategy>();
        services.AddTransient<IEventRequestApprovalStrategy, StudentPlanner.Core.Application.EventRequests.Strategies.DeleteApprovalStrategy>();
        services.AddTransient<IEventPreviewStrategy, PersonalEventPreveiwStrategy>();
        services.AddTransient<IEventPreviewStrategy, AcademicEventPreviewStrategy>();
        services.AddTransient<IEventPreviewStrategy, UsosEventPreviewStrategy>();

        //hosted
        services.AddHostedService<FacultyBootstrapService>();
    }
}
