using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.IdentityEntities;
using FluentAssertions;
using StudentPlanner.Infrastructure;
using StudentPlanner.Infrastructure.Services;

namespace StudentPlanner.Tests.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IAuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _emailServiceMock = new Mock<IEmailService>();
        
        _authService = new AuthenticationService(_userManagerMock.Object, _emailServiceMock.Object);
    }

    [Fact]
    public void RegisterRequestDto_ShouldFailValidation_WhenDomainIsNotPwEduPl()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@gmail.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        isValid.Should().BeFalse();
        results.Should().Contain(x => x.ErrorMessage != null && x.ErrorMessage.Contains("@pw.edu.pl"));
    }

    [Fact]
    public void RegisterRequestDto_ShouldFailValidation_WhenPasswordDoesNotMatchConfirmPassword()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        isValid.Should().BeFalse();
        results.Should().Contain(x => x.ErrorMessage != null && x.ErrorMessage.Contains("match"));
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(new ApplicationUser() { FirstName = "Existing", LastName = "User" });

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenValid()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        
        _userManagerMock.Setup(x => x.CreateAsync(
            It.Is<ApplicationUser>(u => 
                u.UserName == request.Email &&
                u.Email == request.Email && 
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName), 
            request.Password)).ReturnsAsync(IdentityResult.Success);

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Registration Successful");
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenUserManagerFailsToCreateUser()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "password",
            ConfirmPassword = "password",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak!" });
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(failedResult);

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
        result.Message.Should().Contain("Password too weak!");
    }

    [Fact]
    public async Task RegisterAsync_ShouldFail_WhenPasswordDoesNotMeetIdentityPolicy()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@pw.edu.pl",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "John",
            LastName = "Doe"
        };
        
        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);
        
        var failedResult = IdentityResult.Failed(
            new IdentityError { Code = "PasswordRequiresUpper", Description = "Passwords must have at least one uppercase ('A'-'Z')." },
            new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 6 characters." }
        );
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password)).ReturnsAsync(failedResult);

        var result = await _authService.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
        result.Message.Should().Contain("Passwords must have at least one uppercase");
        result.Message.Should().Contain("Passwords must be at least 6 characters");
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new LoginRequestDto
        {
            Email = "nonexistent@pw.edu.pl",
            Password = "SomePassword123!"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid Credentials");
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsIncorrect()
    {
        var request = new LoginRequestDto
        {
            Email = "user@pw.edu.pl",
            Password = "WrongPassword123!"
        };

        var user = new ApplicationUser { Email = request.Email, FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid Credentials");
    }

    [Fact]
    public async Task LoginAsync_ShouldSucceed_AndIdentifyRole_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto
        {
            Email = "user@pw.edu.pl",
            Password = "CorrectPassword123!"
        };

        var user = new ApplicationUser { Email = request.Email, FirstName = "Test", LastName = "User" };
        var roles = new List<string> { "Manager" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles); //todo

        var result = await _authService.LoginAsync(request);

        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty(); //todo
        result.Message.Should().Contain("Manager"); //todo
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldSendEmail_WhenUserExists()
    {
        var request = new ForgotPasswordRequestDto { Email = "user@pw.edu.pl" };
        var user = new ApplicationUser { Email = request.Email, FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("mocked-token");

        await _authService.ForgotPasswordAsync(request);

        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(request.Email, "mocked-token"), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        var request = new ForgotPasswordRequestDto { Email = "nonexistent@pw.edu.pl" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        await _authService.ForgotPasswordAsync(request);

        _emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

        [Fact]
    public void ResetPasswordRequestDto_ShouldFailValidation_WhenPasswordsDoNotMatch()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "user@pw.edu.pl",
            Token = "mocked-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "DifferentPassword!"
        };

        var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

        isValid.Should().BeFalse();
        results.Should().Contain(x => x.ErrorMessage != null && x.ErrorMessage.Contains("match"));
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
        var user = new ApplicationUser { Email = request.Email, FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword)).ReturnsAsync(IdentityResult.Success);

        var result = await _authService.ResetPasswordAsync(request);

        _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword), Times.Once);
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successfully");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldFail_WhenUserNotFound()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "nonexistent@pw.edu.pl",
            Token = "mocked-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUser?)null);

        var result = await _authService.ResetPasswordAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("invalid attempt");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldFail_WhenTokenIsInvalid()
    {
        var request = new ResetPasswordRequestDto
        {
            Email = "user@pw.edu.pl",
            Token = "invalid-token",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };
        var user = new ApplicationUser { Email = request.Email, FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Invalid token." });
        _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.NewPassword)).ReturnsAsync(failedResult);

        var result = await _authService.ResetPasswordAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid token");
    }

   
}
