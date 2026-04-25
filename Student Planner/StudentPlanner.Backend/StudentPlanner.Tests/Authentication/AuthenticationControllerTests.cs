using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.UI.Controllers;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace StudentPlanner.Tests.Authentication;

public class AuthenticationControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<AuthenticationController>> _loggerMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<AuthenticationController>>();
        _controller = new AuthenticationController(_authServiceMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Register_ShouldReturn201_WhenSuccess()
    {

        var request = new RegisterRequestDto { Email = "test@pw.edu.pl", Password = "Password123!" };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).Returns(Task.CompletedTask);


        var result = await _controller.Register(request);


        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailExists()
    {

        var request = new RegisterRequestDto { Email = "exists@pw.edu.pl" };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new InvalidOperationException("A user with this email already exists."));


        var result = await _controller.Register(request);


        var conflictResult = result as ConflictObjectResult;
        conflictResult.Should().NotBeNull();
        conflictResult!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenGenericBadRequest()
    {

        var request = new RegisterRequestDto { Email = "invalid@pw.edu.pl" };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new InvalidOperationException("Invalid input"));


        var result = await _controller.Register(request);


        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn500_WhenDatabaseError()
    {

        var request = new RegisterRequestDto { Email = "db@pw.edu.pl" };
        _authServiceMock.Setup(s => s.RegisterAsync(request)).ThrowsAsync(new Exception("Database connection failed"));


        var result = await _controller.Register(request);


        var statusCodeResult = result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Login_ShouldReturn200_WhenCredentialsValid()
    {

        var request = new LoginRequestDto { Email = "user@pw.edu.pl", Password = "Password123!" };
        var loginResponse = new LoginResponseDto { Token = "jwt-token", Email = request.Email, UserRole = "Student" };
        var refreshResult = new RefreshTokenResult { RefreshToken = "ref-token", ExpirationDate = DateTime.UtcNow.AddDays(7) };

        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync((loginResponse, refreshResult));


        var result = await _controller.Login(request);


        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(loginResponse);
        _controller.Response.Headers["Set-Cookie"].ToString().Should().Contain("refreshToken=ref-token");
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenCredentialsInvalid()
    {

        var request = new LoginRequestDto { Email = "user@pw.edu.pl", Password = "wrong" };
        _authServiceMock.Setup(s => s.LoginAsync(request)).ThrowsAsync(new UnauthorizedAccessException("Invalid Credentials"));


        var result = await _controller.Login(request);


        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturn200_WhenSuccess()
    {

        var request = new ForgotPasswordRequestDto { Email = "any@pw.edu.pl" };
        _authServiceMock.Setup(s => s.ForgotPasswordAsync(request)).Returns(Task.CompletedTask);


        var result = await _controller.ForgotPassword(request);


        result.Should().BeOfType<OkResult>();
        _authServiceMock.Verify(s => s.ForgotPasswordAsync(request), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturn200_WhenEmailNotFound()
    {

        var request = new ForgotPasswordRequestDto { Email = "notfound@pw.edu.pl" };
        _authServiceMock.Setup(s => s.ForgotPasswordAsync(request)).ThrowsAsync(new InvalidOperationException("User not found."));


        var result = await _controller.ForgotPassword(request);


        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturn200_WhenSuccess()
    {

        var request = new ResetPasswordRequestDto { Email = "user@pw.edu.pl", Token = "tok", NewPassword = "New" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request)).Returns(Task.CompletedTask);


        var result = await _controller.ResetPassword(request);


        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturn400_WhenEmailNotFound()
    {

        var request = new ResetPasswordRequestDto { Email = "notfound@pw.edu.pl" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request)).ThrowsAsync(new InvalidOperationException("User not found."));


        var result = await _controller.ResetPassword(request);


        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult!.Value.Should().Be("Invalid or expired token.");
    }

    [Fact]
    public async Task ResetPassword_ShouldReturn400_WhenError()
    {

        var request = new ResetPasswordRequestDto { Email = "user@pw.edu.pl", Token = "invalid" };
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request)).ThrowsAsync(new Exception("Invalid token"));


        var result = await _controller.ResetPassword(request);


        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult!.Value.Should().Be("Invalid or expired token.");
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnActualError_WhenPasswordTooWeak()
    {

        var request = new ResetPasswordRequestDto { Email = "user@pw.edu.pl", Token = "valid-token", NewPassword = "123" };
        var weakPasswordError = "Password must be at least 8 characters.";
        _authServiceMock.Setup(s => s.ResetPasswordAsync(request)).ThrowsAsync(new InvalidOperationException(weakPasswordError));


        var result = await _controller.ResetPassword(request);


        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult!.Value.Should().Be(weakPasswordError);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturn200_WhenValid()
    {

        var oldToken = "old-ref-token";
        var refreshTokenResponse = new RefreshTokenResponse
        {
            AccessToken = "new-jwt",
            RefreshToken = "new-ref-token",
            ExpirationDate = DateTime.UtcNow.AddDays(7)
        };


        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c["refreshToken"]).Returns(oldToken);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(h => h.Request.Cookies).Returns(mockCookies.Object);
        mockHttpContext.Setup(h => h.Response.Cookies).Returns(new Mock<IResponseCookies>().Object); // Just to avoid null ref if used

        _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = $"refreshToken={oldToken}";
        // DefaultHttpContext.Request.Cookies are automatically populated from Headers["Cookie"]

        _authServiceMock.Setup(s => s.RotateRefreshToken(oldToken)).ReturnsAsync(refreshTokenResponse);


        var result = await _controller.RefreshToken();


        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be("new-jwt");
        _controller.Response.Headers["Set-Cookie"].ToString().Should().Contain("refreshToken=new-ref-token");
    }

    [Fact]
    public async Task RefreshToken_ShouldReturn401_WhenNoCookieFound()
    {

        var result = await _controller.RefreshToken();


        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.Value.Should().Be("Session Expired");
    }
}
