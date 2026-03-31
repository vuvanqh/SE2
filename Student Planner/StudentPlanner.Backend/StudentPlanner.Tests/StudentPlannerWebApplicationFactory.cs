using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure;
using System.Net.Http;
using Moq;

namespace StudentPlanner.Tests;

/// <summary>
/// Custom WebApplicationFactory for managing a real SQL Server (LocalDB) environment for tests.
/// </summary>
public class StudentPlannerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName;
    public Mock<IEmailService> EmailServiceMock { get; } = new();

    public StudentPlannerWebApplicationFactory()
    {
        _dbName = "StudentPlanner_Test_" + Guid.NewGuid().ToString("N");
    }

    public string GetConnectionString()
    {
        return $"Data Source=.;Initial Catalog={_dbName};Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Explicitly override configuration keys
        builder.UseSetting("USE_IN_MEMORY_DATABASE", "false");
        builder.UseSetting("ConnectionStrings:Default", GetConnectionString());
        builder.UseSetting("Jwt:SecretKey", "SuperSecretKeyWhichIsVeryLongAndSecure123!");
        builder.UseSetting("Jwt:Issuer", "StudentPlanner");
        builder.UseSetting("Jwt:Audience", "StudentPlanner");
        builder.UseSetting("AllowedOrigins:0", "http://localhost:5173");
        builder.UseSetting("Serilog:MinimumLevel:Default", "Information");
        builder.UseSetting("Serilog:WriteTo:0:Name", "Console");
        builder.UseSetting("RefreshToken:expiration_minutes", "60");
        builder.UseSetting("RefreshToken:max_session_lifetime_days", "30");

        builder.ConfigureLogging(logging => 
        {
            logging.ClearProviders();
            logging.AddConfiguration(new ConfigurationBuilder().Build()); // Empty config for default logging
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace real email service with mock
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);
        });
    }

    /// <summary>
    /// Ensures the database is created and migrated.
    /// This is called explicitly by xUnit before tests run.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure the database is created and all migrations are applied
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureDeletedAsync();
        }
        catch (ObjectDisposedException)
        {
            // Silently ignore if already disposed during teardown
        }
    }
}
