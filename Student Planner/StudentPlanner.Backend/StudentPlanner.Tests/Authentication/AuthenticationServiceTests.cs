using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using FluentAssertions;

namespace StudentPlanner.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly IAuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _emailServiceMock = new Mock<IEmailService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _userRepoMock = new Mock<IUserRepository>();
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();

        _authService = new AuthenticationService(
            _identityServiceMock.Object,
            _emailServiceMock.Object,
            _jwtServiceMock.Object,
            _userRepoMock.Object,
            _refreshTokenServiceMock.Object);
    }



    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        
        var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email, FirstName = "Existing", LastName = "User" };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(existingUser);
        Func<Task> act = async () => await _authService.RegisterAsync(request);
        await act.Should().ThrowAsync<ApplicationException>().WithMessage("A user with this email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValid()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _identityServiceMock.Setup(s => s.RegisterUser(It.IsAny<User>(), request.Password)).Returns(Task.CompletedTask);

        await _authService.RegisterAsync(request);

        _identityServiceMock.Verify(s => s.RegisterUser(
            It.Is<User>(u => u.Email == request.Email && u.FirstName == request.FirstName && u.LastName == request.LastName), 
            request.Password), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenValid()
    {
        var request = new LoginRequestDto { Email = "user@pw.edu.pl", Password = "Password123!" };
        var user = new User { Id = Guid.NewGuid(), Email = request.Email, FirstName = "John", LastName = "Doe" };
        var refreshTokenResult = new RefreshTokenResult { RefreshToken = "ref-token", ExpirationDate = DateTime.UtcNow.AddDays(7) };

        _identityServiceMock.Setup(s => s.SignInAsync(request.Email, request.Password)).ReturnsAsync(user);
        _refreshTokenServiceMock.Setup(r => r.IssueOnLogin(user)).ReturnsAsync(refreshTokenResult);
        _jwtServiceMock.Setup(j => j.CreateToken(user)).Returns("jwt-token");

        var (loginResponse, refreshResult) = await _authService.LoginAsync(request);

        loginResponse.Token.Should().Be("jwt-token");
        loginResponse.Email.Should().Be(user.Email);
        refreshResult.RefreshToken.Should().Be("ref-token");
    }

    [Fact]
    public async Task LoginAsync_ShouldNotGenerateTokens_WhenIdentityServiceThrows()
    {
        var request = new LoginRequestDto { Email = "user@pw.edu.pl", Password = "Wrong" };
        _identityServiceMock.Setup(s => s.SignInAsync(request.Email, request.Password))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid Credentials"));
        Func<Task> act = async () => await _authService.LoginAsync(request);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        
        _jwtServiceMock.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenServiceMock.Verify(r => r.IssueOnLogin(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldSendEmail_WhenUserExists()
    {
        var request = new ForgotPasswordRequestDto { Email = "user@pw.edu.pl" };
        var user = new User { Id = Guid.NewGuid(), Email = request.Email, FirstName = "John", LastName = "Doe" };

        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
        _identityServiceMock.Setup(i => i.GeneratePasswordResetTokenAsync(user.Email)).ReturnsAsync("mocked-token");

        await _authService.ForgotPasswordAsync(request);

        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(request.Email, "mocked-token"), Times.Once);
    }
    
    [Fact]
    public async Task ForgotPasswordAsync_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        var request = new ForgotPasswordRequestDto { Email = "nonexistent@pw.edu.pl" };
        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        await _authService.ForgotPasswordAsync(request);

        _identityServiceMock.Verify(i => i.GeneratePasswordResetTokenAsync(request.Email), Times.Never);
        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSucceed_WhenValid()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "user@pw.edu.pl",
            Token = "mocked-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var user = new User { Id = Guid.NewGuid(), Email = request.Email, FirstName = "John", LastName = "Doe" };

        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
        _identityServiceMock.Setup(i => i.ResetPasswordAsync(request.Email, request.Token, request.NewPassword)).Returns(Task.CompletedTask);

        await _authService.ResetPasswordAsync(request);

        _identityServiceMock.Verify(x => x.ResetPasswordAsync(request.Email, request.Token, request.NewPassword), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        var request = new ResetPasswordRequestDto { Email = "ghost@pw.edu.pl", Token = "tok", NewPassword = "New" };
        
        _userRepoMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        Func<Task> act = async () => await _authService.ResetPasswordAsync(request);
        await act.Should().ThrowAsync<ApplicationException>();
        
        _identityServiceMock.Verify(i => i.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public async Task RotateRefreshToken_ShouldReturnNewTokens_WhenValid()
    {
        var oldToken = "old-ref-token";
        var user = new User { Id = Guid.NewGuid(), Email = "user@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        var newRefreshResult = new RefreshTokenResult { RefreshToken = "new-ref-token", ExpirationDate = DateTime.UtcNow.AddDays(7) };

        _refreshTokenServiceMock.Setup(r => r.RotateTokenAsync(oldToken)).ReturnsAsync((user, newRefreshResult));
        _jwtServiceMock.Setup(j => j.CreateToken(user)).Returns("new-jwt-token");

        var response = await _authService.RotateRefreshToken(oldToken);

        response.AccessToken.Should().Be("new-jwt-token");
        response.RefreshToken.Should().Be("new-ref-token");
        response.ExpirationDate.Should().Be(newRefreshResult.ExpirationDate);
    }

    [Fact]
    public async Task RotateRefreshToken_ShouldNotGenerateJwt_WhenRefreshTokenIsInvalid()
    {
        var oldToken = "invalid-token";
        _refreshTokenServiceMock.Setup(r => r.RotateTokenAsync(oldToken))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid refresh token"));
        Func<Task> act = async () => await _authService.RotateRefreshToken(oldToken);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _jwtServiceMock.Verify(j => j.CreateToken(It.IsAny<User>()), Times.Never);
    }
}
