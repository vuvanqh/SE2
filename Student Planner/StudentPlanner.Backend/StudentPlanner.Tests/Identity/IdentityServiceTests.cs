using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Infrastructure.IdentityEntities;
using StudentPlanner.Infrastructure.Identity;
using FluentAssertions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using StudentPlanner.Core.Entities;
using System.Linq;

namespace StudentPlanner.Tests.Identity;

public class IdentityServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly IIdentityService _identityService;

    public IdentityServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(_userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);

        _identityService = new IdentityService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    [Fact]
    public async Task SignInAsync_ShouldThrowException_WhenUserNotFound()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync((ApplicationUser?)null);

        Func<Task> act = async () => await _identityService.SignInAsync("test@pw.edu.pl", "Password123!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid Credentials");
    }

    [Fact]
    public async Task SignInAsync_ShouldThrowException_WhenPasswordIsIncorrect()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", UserName = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(appUser, "WrongPassword!", false, true))
            .ReturnsAsync(SignInResult.Failed);

        Func<Task> act = async () => await _identityService.SignInAsync("test@pw.edu.pl", "WrongPassword!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid Credentials");
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        var appUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(appUser, "Password123!", false, true))
            .ReturnsAsync(SignInResult.Success);

        var user = await _identityService.SignInAsync("test@pw.edu.pl", "Password123!");

        user.Should().NotBeNull();
        user.Email.Should().Be(appUser.Email);
        user.FirstName.Should().Be(appUser.FirstName);
        user.LastName.Should().Be(appUser.LastName);
    }

    [Fact]
    public async Task RegisterUser_ShouldThrowException_WhenUserManagerFails()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        var failedResult = IdentityResult.Failed(
            new IdentityError { Description = "Password too weak!" },
            new IdentityError { Description = "Password requires uppercase." }
        );
        
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "password")).ReturnsAsync(failedResult);

        Func<Task> act = async () => await _identityService.RegisterUser(user, "password");

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Password too weak!, Password requires uppercase.");
    }

    [Fact]
    public async Task RegisterUser_ShouldSucceed_WhenUserManagerSucceeds()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "password")).ReturnsAsync(IdentityResult.Success);

        Func<Task> act = async () => await _identityService.RegisterUser(user, "password");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldReturnToken_WhenUserExists()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(appUser)).ReturnsAsync("mocked-token");

        var token = await _identityService.GeneratePasswordResetTokenAsync("test@pw.edu.pl");

        token.Should().Be("mocked-token");
    }

      [Fact]
    public async Task GeneratePasswordResetTokenAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("ghost@pw.edu.pl")).ReturnsAsync((ApplicationUser?)null);

        Func<Task> act = async () => await _identityService.GeneratePasswordResetTokenAsync("ghost@pw.edu.pl");

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Invalid Operation");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("ghost@pw.edu.pl")).ReturnsAsync((ApplicationUser?)null);

        Func<Task> act = async () => await _identityService.ResetPasswordAsync("ghost@pw.edu.pl", "token", "NewPassword123!");

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Invalid Operation");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSucceed_WhenValid()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        _userManagerMock.Setup(x => x.ResetPasswordAsync(appUser, "mocked-token", "NewPassword123!")).ReturnsAsync(IdentityResult.Success);

        Func<Task> act = async () => await _identityService.ResetPasswordAsync("test@pw.edu.pl", "mocked-token", "NewPassword123!");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenTokenIsInvalid()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Invalid token." });
        _userManagerMock.Setup(x => x.ResetPasswordAsync(appUser, "invalid-token", "NewPassword123!")).ReturnsAsync(failedResult);

        Func<Task> act = async () => await _identityService.ResetPasswordAsync("test@pw.edu.pl", "invalid-token", "NewPassword123!");

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Invalid token.");
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldThrowException_WhenPasswordIsWeak()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        
        var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak!" });
        _userManagerMock.Setup(x => x.ResetPasswordAsync(appUser, "valid-token", "weak")).ReturnsAsync(failedResult);

        Func<Task> act = async () => await _identityService.ResetPasswordAsync("test@pw.edu.pl", "valid-token", "weak");

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Password too weak!");
    }

    [Fact]
    public async Task UpdateToken_ShouldUpdateUser_WhenUserExists()
    {
        var appUser = new ApplicationUser { Email = "test@pw.edu.pl", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@pw.edu.pl")).ReturnsAsync(appUser);
        
        var expiration = DateTime.UtcNow.AddDays(7);
        Func<Task> act = async () => await _identityService.UpdateToken("test@pw.edu.pl", "token-hash", expiration);

        await act.Should().NotThrowAsync();
        
        appUser.RefreshTokenHash.Should().Be("token-hash");
        appUser.RefreshTokenExpirationDate.Should().Be(expiration);
        _userManagerMock.Verify(x => x.UpdateAsync(appUser), Times.Once);
    }


    [Fact]
    public async Task UpdateToken_ShouldThrowException_WhenUserDoesNotExist()
    {
        _userManagerMock.Setup(x => x.FindByEmailAsync("ghost@pw.edu.pl")).ReturnsAsync((ApplicationUser?)null);

        Func<Task> act = async () => await _identityService.UpdateToken("ghost@pw.edu.pl", "hash", DateTime.UtcNow);

        await act.Should().ThrowAsync<ApplicationException>().WithMessage("Invalid Operation");
    }
}
