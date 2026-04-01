using Microsoft.AspNetCore.Mvc.Testing;
using StudentPlanner.Backend;
using StudentPlanner.Core.Application.Authentication;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using StudentPlanner.Infrastructure.IdentityEntities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace StudentPlanner.Tests.Authentication;

/// <summary>
/// End-to-end tests for the AuthenticationController.
/// </summary>
public class AuthenticationControllerE2ETests : IntegrationTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationControllerE2ETests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory.</param>
    public AuthenticationControllerE2ETests(StudentPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_Success_201()
    {

        var request = new RegisterRequestDto
        {
            Email = "newuser@pw.edu.pl",
            Password = "Password123!"
        };


        var response = await _client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);


        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Expected Created but got {response.StatusCode}. Body: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_DuplicateEmail_409()
    {
        var request = new RegisterRequestDto
        {
            Email = "duplicate@pw.edu.pl",
            Password = "Password123!"
        };
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await _client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.Conflict)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Expected Conflict but got {response.StatusCode}. Body: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_InvalidPassword_400()
    {
        var request = new RegisterRequestDto
        {
            Email = "invalidpass@pw.edu.pl",
            Password = "1" // too short, should fail identity rules
        };
        var response = await _client.PostAsJsonAsync("/api/auth/register", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Success_200()
    {
        var email = "loginuser@pw.edu.pl";
        var password = "Password123!";
        var loginRequest = new LoginRequestDto { Email = email, Password = password };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Registration failed in Login_Success_200. Status: {registerResponse.StatusCode}, Body: {body}");
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(TestContext.Current.CancellationToken);
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Email.Should().Be(email);

        response.Headers.Contains("Set-Cookie").Should().BeTrue();
        response.Headers.GetValues("Set-Cookie").Should().ContainMatch("*refreshToken=*");
    }

    [Fact]
    public async Task Login_InvalidCredentials_401()
    {
        var loginRequest = new LoginRequestDto { Email = "nonexistent@pw.edu.pl", Password = "WrongPassword" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_EmailFound_200()
    {
        var request = new ForgotPasswordRequestDto { Email = "any@pw.edu.pl" };

        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_EmailNotFound_200()
    {
        var request = new ForgotPasswordRequestDto { Email = "nonexistent@pw.edu.pl" };
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_Success_200()
    {
        var email = "reset@pw.edu.pl";
        var password = "OldPassword123!";
        var newPassword = "NewPassword123!";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Registration failed in ResetPassword_Success_200. Status: {registerResponse.StatusCode}, Body: {body}");
        }

        await _client.PostAsJsonAsync("/api/auth/reset-password", new ForgotPasswordRequestDto { Email = email }, TestContext.Current.CancellationToken);

        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception($"User {email} not found in DB after registration in ResetPassword_Success_200.");
            }
            token = await userManager.GeneratePasswordResetTokenAsync(user);
        }

        var resetRequest = new ResetPasswordRequestDto
        {
            Email = email,
            Token = token,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };

        var response = await _client.PostAsJsonAsync("/api/auth/verify-reset", resetRequest, TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Expected OK but got {response.StatusCode}. Body: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = newPassword }, TestContext.Current.CancellationToken);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_UserNotFound_404()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "missing-user@pw.edu.pl",
            Token = "some-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/verify-reset", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_400()
    {
        var email = "valid-user@pw.edu.pl";
        var password = "OldPassword123!";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Registration failed in ResetPassword_InvalidToken_400. Status: {registerResponse.StatusCode}, Body: {body}");
        }

        var request = new ResetPasswordRequestDto
        {
            Email = email,
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/verify-reset", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_Success_200()
    {
        var email = "refresh@pw.edu.pl";
        var password = "Password123!";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);
        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Registration failed in RefreshToken_Success_200. Status: {registerResponse.StatusCode}, Body: {body}");
        }

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password }, TestContext.Current.CancellationToken);
        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Login failed in RefreshToken_Success_200. Status: {loginResponse.StatusCode}, Body: {body}");
        }

        var response = await _client.PostAsync("/api/auth/refreshToken", null, TestContext.Current.CancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Expected OK but got {response.StatusCode}. Body: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newToken = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        newToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_NoCookie_401()
    {
        var response = await _client.PostAsync("/api/auth/refreshToken", null, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_401()
    {

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refreshToken");
        request.Headers.Add("Cookie", "refreshToken=invalid-token");
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


}
