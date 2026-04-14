using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure;
using StudentPlanner.Core.Application.ClientContracts;
using System.Net.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace StudentPlanner.Tests;

/// <summary>
/// Custom WebApplicationFactory for managing a real SQL Server (LocalDB) environment for tests.
/// </summary>
public class StudentPlannerWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName;

    /// <summary>
    /// Gets the mock for the IEmailService.
    /// </summary>
    public Mock<IEmailService> EmailServiceMock { get; } = new();

    /// <summary>
    /// Gets the mock for the IUsosClient.
    /// </summary>
    public Mock<IUsosClient> UsosAuthServiceMock { get; } = new();

    public StudentPlannerWebApplicationFactory()
    {
        _dbName = "StudentPlanner_Test_" + Guid.NewGuid().ToString("N");

        // Default mock setup for USOS login
        UsosAuthServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UsosLoginResponse
            {
                FirstName = "Test",
                LastName = "User",
                FacultyId = "test-faculty",
                Token = "test-token"
            });

        UsosAuthServiceMock.Setup(s => s.GetFacultiesAsync())
            .ReturnsAsync(new List<StudentPlanner.Core.Domain.Entities.Faculty>
            {
                new StudentPlanner.Core.Domain.Entities.Faculty
                {
                    FacultyId = "test-faculty",
                    FacultyName = "Test Faculty",
                    FacultyCode = "TF",
                    Id = Guid.NewGuid()
                }
            });
    }

    /// <summary>
    /// Gets the connection string for the test-specific database.
    /// </summary>
    /// <returns>The connection string.</returns>
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
            logging.AddConsole();
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace real email service with mock
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);

            // Replace real USOS service with mock
            services.RemoveAll<IUsosClient>();
            services.AddScoped<IUsosClient>(_ => UsosAuthServiceMock.Object);
        });
    }

    /// <summary>
    /// Ensures the database is created and migrated.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // migrate the database before the services
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;

        using var db = new ApplicationDbContext(options);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // ensures the services are available for tests
        _ = Services;
    }

    /// <summary>
    /// Cleans up the test-specific database and base resources.
    /// </summary>
    /// <returns>A value task.</returns>
    public override async ValueTask DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider?.GetService<ApplicationDbContext>();
            if (db != null)
            {
                await db.Database.EnsureDeletedAsync();
            }
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }

        await base.DisposeAsync();
    }
}
