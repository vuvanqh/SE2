using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application.Authentication;
using Moq;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Infrastructure;
using System.Collections.Generic;
using System;
using System.Net.Http;

namespace StudentPlanner.Tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly Mock<IEmailService> _emailServiceMock = new();

    private readonly string _dbName = Guid.NewGuid().ToString();

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "true",
                    ["Jwt:Issuer"] = "StudentPlanner",
                    ["Jwt:Audience"] = "StudentPlanner",
                    ["Jwt:Key"] = "SuperSecretKeyWhichIsVeryLongAndSecure123!",
                    ["Jwt:SecretKey"] = "SuperSecretKeyWhichIsVeryLongAndSecure123!",
                    ["Jwt:Expiration_Minutes"] = "60",
                    ["RefreshToken:expiration_minutes"] = "1440",
                    ["RefreshToken:max_session_lifetime_days"] = "7"
                });
            });

            builder.ConfigureServices(services =>
            {
                //isolate database for each test
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                services.RemoveAll<IEmailService>();
                services.AddScoped<IEmailService>(_ => _emailServiceMock.Object);
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
    }
}
