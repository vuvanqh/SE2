using Serilog;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Infrastructure;

namespace StudentPlanner.Backend;

using Microsoft.EntityFrameworkCore;
using StudentPlanner.Infrastructure;
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
    public static void Main(string[] args)
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
            db.Database.Migrate();
        }

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

        app.MapControllers();

        app.Run();
    }
}
