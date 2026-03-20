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

   
}
