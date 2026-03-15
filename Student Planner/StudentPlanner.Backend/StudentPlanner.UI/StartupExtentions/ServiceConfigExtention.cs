using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Infrastructure.Repositories;

namespace StudentPlanner.UI;

public static class ServiceConfigExtention
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration config)
    {
        //services

        //repo
        services.AddScoped<IPersonalEventRepository, PersonalEventRepository>();
    }
}
