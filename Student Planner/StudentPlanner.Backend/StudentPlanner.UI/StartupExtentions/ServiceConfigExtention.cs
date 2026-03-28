using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.PersonalEvents;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Infrastructure.Identity;
using StudentPlanner.Infrastructure.Repositories;

namespace StudentPlanner.UI;

public static class ServiceConfigExtention
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        //services
        services.AddScoped<IPersonalEventService, PersonalEventService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailService, EmailService>();

        //repo
        services.AddScoped<IPersonalEventRepository, PersonalEventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
