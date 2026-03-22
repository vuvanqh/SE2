
using Serilog;
using StudentPlanner.UI;

namespace StudentPlanner.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureBaseline(builder.Configuration);
        builder.Services.ConfigureServices(builder.Configuration);

        //serilog
        builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration) //give serilog permission to read the config from appsettings.json
                               .ReadFrom.Services(services); //read the services & make them available to the serilog
        });

        var app = builder.Build();

        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }
}
