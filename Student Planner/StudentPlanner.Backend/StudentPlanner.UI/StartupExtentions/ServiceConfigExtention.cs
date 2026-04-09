using MailKit.Net.Smtp;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.Services.Settings;
using StudentPlanner.Core.Application.PersonalEvents;
using StudentPlanner.Core.Application.EventRequests;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Infrastructure.Identity;
using StudentPlanner.Infrastructure.Repositories;
using StudentPlanner.Infrastructure.Services;

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
        services.AddTransient<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, MailtrapEmailService>();

        var baseUrl = config["UsosApi:BaseUrl"];

        Console.WriteLine($"DEBUG BaseUrl = {baseUrl}");

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

        services.AddScoped<StudentPlanner.Core.Application.EventRequests.Strategies.CreateApprovalStrategy>();
        services.AddScoped<StudentPlanner.Core.Application.EventRequests.Strategies.UpdateApprovalStrategy>();
        services.AddScoped<StudentPlanner.Core.Application.EventRequests.Strategies.DeleteApprovalStrategy>();

        //hosted
        services.AddHostedService<FacultyBootstrapService>();
    }
}
