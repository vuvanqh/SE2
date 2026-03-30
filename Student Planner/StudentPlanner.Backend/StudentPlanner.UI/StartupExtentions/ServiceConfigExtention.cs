using MailKit.Net.Smtp;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.Services.Settings;
using StudentPlanner.Core.Application.PersonalEvents;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Infrastructure.Identity;
using StudentPlanner.Infrastructure.Repositories;
using StudentPlanner.Infrastructure.Services;

namespace StudentPlanner.Backend;

public static class ServiceConfigExtention
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        //mail
        services.AddTransient<ISmtpClient, SmtpClient>();

        //services
        services.AddScoped<IPersonalEventService, PersonalEventService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, MailtrapEmailService>();

        //repo
        services.AddScoped<IPersonalEventRepository, PersonalEventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
