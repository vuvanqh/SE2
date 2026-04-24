using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.Testing;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using StudentPlanner.Infrastructure.Services;

namespace StudentPlanner.Tests.Authentication;

public class EmailIntegrationTests : IntegrationTestBase
{
    private new readonly HttpClient _client;

    public EmailIntegrationTests(StudentPlannerWebApplicationFactory factory) : base(factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEmailService>();
                services.AddScoped<IEmailService, MailtrapEmailService>();
            });
        });

        _client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task ForgotPassword_ShouldSendRealEmail_AndTokenShouldBeValid()
    {
        var ct = TestContext.Current.CancellationToken;

        // skip if credentials are not configured
        var apiToken = Environment.GetEnvironmentVariable("MAILTRAP_API_TOKEN");
        if (string.IsNullOrWhiteSpace(apiToken) || apiToken == "your_token_here")
        {
            Assert.Skip("Mailtrap credentials not configured in .env");
        }

        // Only resolve the client if we have credentials, to avoid constructor exceptions
        var mailtrapClient = _factory.Services.GetRequiredService<MailtrapTestingClient>();

        var timestamp = DateTime.UtcNow.AddMinutes(-1);
        var email = "integration@pw.edu.pl";
        var password = "OldPassword123!";


        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, ct);

        if (regResponse.StatusCode != HttpStatusCode.Created && regResponse.StatusCode != HttpStatusCode.Conflict)
        {
            var regError = await regResponse.Content.ReadAsStringAsync(ct);
            throw new Exception($"Registration failed: {regResponse.StatusCode}, Error: {regError}");
        }

        // password reset
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", new ForgotPasswordRequestDto { Email = email }, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string? token = null;
        MailtrapMessage? foundMessage = null;

        var subjectsFound = new List<string>();
        for (int i = 0; i < 20; i++) // 40s, 2s intervals
        {
            var messages = await mailtrapClient.GetMessagesAsync(ct);
            subjectsFound = messages.Select(m => $"{m.Subject} (at {m.CreatedAt:HH:mm:ss})").ToList();

            foundMessage = messages
                .Where(m => m.Subject.Contains("Reset your password") && m.CreatedAt >= timestamp)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (foundMessage != null)
            {
                var fullMessage = await mailtrapClient.GetMessageAsync(foundMessage.Id, ct);
                if (fullMessage != null)
                {
                    token = ExtractToken(fullMessage.TextBody);
                    if (!string.IsNullOrEmpty(token)) break;
                }
            }

            await Task.Delay(2000, ct);
        }

        if (string.IsNullOrEmpty(token))
        {
            var msg = $"Reset token not found in Mailtrap inbox within timeout.\n" +
                      $"Timestamp: {timestamp:HH:mm:ss}\n" +
                      $"Available messages:\n{string.Join("\n", subjectsFound)}";
            throw new Exception(msg);
        }

        // verify reset
        var newPassword = "NewPassword123!";
        var resetRequest = new ResetPasswordRequestDto
        {
            Email = email,
            Token = token!,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };

        var resetResponse = await _client.PostAsJsonAsync("/api/auth/verify-reset", resetRequest, ct);
        if (resetResponse.StatusCode != HttpStatusCode.OK)
        {
            var error = await resetResponse.Content.ReadAsStringAsync(ct);
            throw new Exception($"Reset verification failed! Status={resetResponse.StatusCode}, Error={error}");
        }

        // verify login 
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = newPassword }, ct);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string ExtractToken(string body)
    {
        if (string.IsNullOrEmpty(body)) return string.Empty;

        var match = Regex.Match(body, @"Password reset code:\s*([^\s\r\n]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return string.Empty;
    }
}
