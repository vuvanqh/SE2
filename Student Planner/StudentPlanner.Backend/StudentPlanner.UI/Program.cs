using Serilog;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Infrastructure;
using StudentPlanner.Infrastructure.Identity;
using StudentPlanner.UI.Hubs;

namespace StudentPlanner.Backend;


/// <summary>
/// Entry point for the StudentPlanner application.
/// </summary>
public class Program
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Program"/> class.
    /// </summary>
    protected Program() { }

    /// <summary>
    /// The main entry point of the application.
    /// </summary>
    /// <param name="args">Arguments passed to the application.</param>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureBaseline(builder.Configuration);
        builder.Services.ConfigureServices(builder.Configuration);


        builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                               .ReadFrom.Services(services);
        });

        var app = builder.Build();
        if (!app.Environment.IsEnvironment("Testing"))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }

        await IdentitySeeder.SeedAsync(app.Services);

        app.UseRouting();

        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSerilogRequestLogging();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudentPlanner API v1");
        });

        app.MapHub<EventRequestHub>("/hubs/eventRequest");
        app.MapControllers();

        await app.RunAsync();
    }
}
