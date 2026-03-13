
using StudentPlanner.UI;

namespace StudentPlanner.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureBaseline(builder.Configuration);
        builder.Services.ConfigureServices(builder.Configuration);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
