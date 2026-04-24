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
using StudentPlanner.Infrastructure.Testing;
using StudentPlanner.Infrastructure.Services.Settings;
using Microsoft.Extensions.Options;

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

    /// <summary>
    /// Gets or sets a value indicating whether to use the real email service.
    /// </summary>
    public bool UseRealEmailService { get; set; } = false;

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
                    Id = Guid.Parse("ff8c5ad6-e743-4756-aaf9-7f56d686e57f")
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

        // override EmailSettings from environment variables if present
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // load .env file if it exists
            var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
            string? envPath = null;
            while (currentDir != null)
            {
                var potentialPath = Path.Combine(currentDir.FullName, ".env");
                if (File.Exists(potentialPath))
                {
                    envPath = potentialPath;
                    break;
                }
                currentDir = currentDir.Parent;
            }

            if (envPath == null)
            {
                throw new FileNotFoundException("Could not find .env file in any parent directory of " + AppContext.BaseDirectory);
            }

            foreach (var line in File.ReadAllLines(envPath))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }

            config.AddEnvironmentVariables();

            // Explicitly map MAILTRAP environment variables to EmailSettings properties
            var mailtrapToken = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN");
            var mailtrapInbox = Environment.GetEnvironmentVariable("MAILTRAP_INBOX_ID");
            var mailtrapAccount = Environment.GetEnvironmentVariable("MAILTRAP_ACCOUNT_ID");

            var dict = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(mailtrapToken)) dict["EmailSettings:ApiToken"] = mailtrapToken.Trim();
            if (!string.IsNullOrWhiteSpace(mailtrapInbox)) dict["EmailSettings:InboxId"] = mailtrapInbox.Trim();
            if (!string.IsNullOrWhiteSpace(mailtrapAccount)) dict["EmailSettings:AccountId"] = mailtrapAccount.Trim();

            if (dict.Any())
            {
                config.AddInMemoryCollection(dict);
            }

            // Map common ENV names to our config structure if they differ
            var overrides = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAILTRAP_USERNAME")))
                overrides["EmailSettings:SmtpUsername"] = Environment.GetEnvironmentVariable("MAILTRAP_USERNAME")!;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAILTRAP_PASSWORD")))
                overrides["EmailSettings:SmtpPassword"] = Environment.GetEnvironmentVariable("MAILTRAP_PASSWORD")!;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN")))
                overrides["EmailSettings:ApiToken"] = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN")!;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MAILTRAP_INBOX_ID")))
                overrides["EmailSettings:InboxId"] = Environment.GetEnvironmentVariable("MAILTRAP_INBOX_ID")!;

            if (overrides.Count > 0)
                config.AddInMemoryCollection(overrides!);
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace real email service with mock UNLESS we want to test integration
            if (!UseRealEmailService)
            {
                services.RemoveAll<IEmailService>();
                services.AddScoped<IEmailService>(_ => EmailServiceMock.Object);
            }

            // register MailtrapTestingClient for E2E tests
            services.AddHttpClient<MailtrapTestingClient>(client =>
            {
                client.BaseAddress = new Uri("https://mailtrap.io/");
            });

            // replace real USOS service with mock
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

        // Seed the required faculty manually before starting the host to satisfy FK constraints in IdentitySeeder
        var faculty = new StudentPlanner.Infrastructure.IdentityEntities.AppFaculty
        {
            Id = Guid.Parse("ff8c5ad6-e743-4756-aaf9-7f56d686e57f"),
            FacultyId = "test-faculty",
            FacultyName = "Test Faculty",
            FacultyCode = "TF"
        };
        await db.Faculties.AddAsync(faculty);
        await db.SaveChangesAsync();

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
