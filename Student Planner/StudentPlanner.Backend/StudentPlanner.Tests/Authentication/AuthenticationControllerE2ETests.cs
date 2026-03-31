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

namespace StudentPlanner.Tests.Authentication;

public class AuthenticationControllerE2ETests : IntegrationTestBase
{
    public AuthenticationControllerE2ETests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_Success_201()
    {

        var request = new RegisterRequestDto
        {
            Email = "newuser@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
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
            Password = "Password123!",
            ConfirmPassword = "Password123!"
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
    public async Task Login_Success_200()
    {
        var email = "loginuser@pw.edu.pl";
        var password = "Password123!";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        }, TestContext.Current.CancellationToken);

        var loginRequest = new LoginRequestDto { Email = email, Password = password };

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
    public async Task ForgotPassword_AlwaysSuccess_200()
    {
        var request = new ForgotPasswordRequestDto { Email = "any@pw.edu.pl" };

        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_Success_200()
    {
        var email = "refresh@pw.edu.pl";
        var password = "Password123!";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        }, TestContext.Current.CancellationToken);

        await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password }, TestContext.Current.CancellationToken);

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
    public async Task ResetPassword_Success_200()
    {
        var email = "reset@pw.edu.pl";
        var password = "OldPassword123!";
        var newPassword = "NewPassword123!";

        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        }, TestContext.Current.CancellationToken);

        await _client.PostAsJsonAsync("/api/auth/reset-password", new ForgotPasswordRequestDto { Email = email }, TestContext.Current.CancellationToken);

        string token;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            token = await userManager.GeneratePasswordResetTokenAsync(user!);
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
}

