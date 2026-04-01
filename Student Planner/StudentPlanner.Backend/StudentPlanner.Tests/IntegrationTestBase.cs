using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application.Authentication;
using Moq;
using Xunit;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace StudentPlanner.Tests;

/// <summary>
/// Base class for all integration tests, providing a shared SQL Server environment.
/// </summary>
public class IntegrationTestBase : IClassFixture<StudentPlannerWebApplicationFactory>, IAsyncLifetime
{
    protected readonly StudentPlannerWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly Mock<IEmailService> _emailServiceMock;

    public IntegrationTestBase(StudentPlannerWebApplicationFactory factory)
    {
        _factory = factory;
        _emailServiceMock = factory.EmailServiceMock;

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
