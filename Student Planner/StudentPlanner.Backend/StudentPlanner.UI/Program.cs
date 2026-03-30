using Serilog;

namespace StudentPlanner.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureBaseline(builder.Configuration);
        builder.Services.ConfigureServices(builder.Configuration);


        builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration)
                               .ReadFrom.Services(services);
        });

        var app = builder.Build();

        app.UseRouting();

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.UseSerilogRequestLogging();

        app.UseCors("AllowFrontend");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "StudentPlanner API v1");
        });

        app.MapControllers();

        app.Run();
    }
}
