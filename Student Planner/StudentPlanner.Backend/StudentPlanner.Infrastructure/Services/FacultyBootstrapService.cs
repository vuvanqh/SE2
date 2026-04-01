using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentPlanner.Core.Application.Authentication;

namespace StudentPlanner.Infrastructure.Services;

public class FacultyBootstrapService : IHostedService
{
    private readonly IUsosClient _usosClient;
    private readonly IServiceScopeFactory _scopedServices;

    public FacultyBootstrapService(IUsosClient usosClient, IServiceScopeFactory scopedServices)
    {
        _usosClient = usosClient;
        _scopedServices = scopedServices;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopedServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.Faculties.AnyAsync()) return;

        var faculties = (await _usosClient.GetFacultiesAsync()).Select(f => new IdentityEntities.AppFaculty
        {
            FacultyCode = f.FacultyCode,
            FacultyId = f.FacultyId,
            FacultyName = f.FacultyName,
            Id = f.Id
        });
        await context.Faculties.AddRangeAsync(faculties);
        await context.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
