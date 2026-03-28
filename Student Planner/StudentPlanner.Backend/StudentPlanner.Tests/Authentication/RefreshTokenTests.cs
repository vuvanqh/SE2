using FluentAssertions;
using Moq;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Tests.Authentication;

public class RefreshTokenServiceTests
{
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly RefreshTokenService _sut;

    public RefreshTokenServiceTests()
    {
        _mockJwtService = new Mock<IJwtService>();
        _mockIdentityService = new Mock<IIdentityService>();
        _mockUserRepository = new Mock<IUserRepository>();

        _sut = new RefreshTokenService(
            _mockJwtService.Object,
            _mockIdentityService.Object,
            _mockUserRepository.Object);
    }

    [Fact]
    public void HashToken_ShouldReturnBase64String_WhenGivenValidToken()
    {
        // Arrange
        var token = "test-refresh-token-123";

        // Act
        var hash = _sut.HashToken(token);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().MatchRegex("^[A-Za-z0-9+/=]+$"); // Base64 format
    }

    [Fact]
    public void HashToken_ShouldReturnSameHash_ForSameInput()
    {
        // Arrange
        var token = "same-token";

        // Act
        var hash1 = _sut.HashToken(token);
        var hash2 = _sut.HashToken(token);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashToken_ShouldReturnDifferentHash_ForDifferentInputs()
    {
        // Arrange
        var token1 = "token-123";
        var token2 = "token-456";

        // Act
        var hash1 = _sut.HashToken(token1);
        var hash2 = _sut.HashToken(token2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public async Task RotateTokenAsync_WithValidToken_ShouldReturnUserAndNewRefreshToken()
    {
        // Arrange
        var currentToken = "valid-refresh-token";
        var hashedToken = _sut.HashToken(currentToken);
        var user = CreateTestUser();
        var newRefreshTokenResult = new RefreshTokenResult
        {
            RefreshToken = "new-refresh-token",
            ExpirationDate = DateTime.UtcNow.AddDays(7)
        };

        // Set valid dates
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7);
        user.RefreshTokenIssuedAt = DateTime.UtcNow.AddDays(-1);

        _mockUserRepository
            .Setup(x => x.GetUserByRefreshToken(hashedToken))
            .ReturnsAsync(user);

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshTokenResult);

        _mockJwtService
            .Setup(x => x.GetMaxSessionLifetimeDays())
            .Returns(7.0);

        _mockIdentityService
            .Setup(x => x.UpdateToken(user.Email, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RotateTokenAsync(currentToken);

        // Assert
        result.Should().NotBeNull();
        result.Item2.Should().Be(newRefreshTokenResult);

        _mockIdentityService.Verify(
            x => x.UpdateToken(user.Email, It.IsAny<string>(), newRefreshTokenResult.ExpirationDate),
            Times.Once);
    }

    [Fact]
    public async Task RotateTokenAsync_WithInvalidToken_ShouldThrowApplicationException()
    {
        // Arrange
        var invalidToken = "invalid-refresh-token";
        var hashedToken = _sut.HashToken(invalidToken);

        _mockUserRepository
            .Setup(x => x.GetUserByRefreshToken(hashedToken))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _sut.RotateTokenAsync(invalidToken));

        exception.Message.Should().Be("Invalid Token");
    }

    [Fact]
    public async Task RotateTokenAsync_WithExpiredRefreshToken_ShouldThrowApplicationException()
    {
        // Arrange
        var currentToken = "expired-refresh-token";
        var hashedToken = _sut.HashToken(currentToken);
        var user = CreateTestUser();

        // Set expired date
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(-1);

        _mockUserRepository
            .Setup(x => x.GetUserByRefreshToken(hashedToken))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _sut.RotateTokenAsync(currentToken));

        exception.Message.Should().Be("Refresh token expired");
    }

    [Fact]
    public async Task RotateTokenAsync_WithExpiredSession_ShouldThrowApplicationException()
    {
        // Arrange
        var currentToken = "session-expired-token";
        var hashedToken = _sut.HashToken(currentToken);
        var user = CreateTestUser();
        var maxSessionDays = 7;

        // Set valid expiration but session expired
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(1);
        user.RefreshTokenIssuedAt = DateTime.UtcNow.AddDays(-maxSessionDays - 1);

        _mockUserRepository
            .Setup(x => x.GetUserByRefreshToken(hashedToken))
            .ReturnsAsync(user);

        _mockJwtService
            .Setup(x => x.GetMaxSessionLifetimeDays())
            .Returns(maxSessionDays);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _sut.RotateTokenAsync(currentToken));

        exception.Message.Should().Be("Session Expired");
    }

    [Fact]
    public async Task IssueOnLogin_ShouldUpdateUserRefreshTokenAndReturnResult()
    {
        // Arrange
        var user = CreateTestUser();
        var expectedRefreshTokenResult = new RefreshTokenResult
        {
            RefreshToken = "new-refresh-token",
            ExpirationDate = DateTime.UtcNow.AddDays(7)
        };

        _mockJwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshTokenResult);

        _mockIdentityService
            .Setup(x => x.UpdateToken(user.Email, It.IsAny<string>(), expectedRefreshTokenResult.ExpirationDate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.IssueOnLogin(user);

        // Assert
        result.Should().Be(expectedRefreshTokenResult);
        user.RefreshTokenIssuedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockIdentityService.Verify(
            x => x.UpdateToken(user.Email, It.IsAny<string>(), expectedRefreshTokenResult.ExpirationDate),
            Times.Once);
    }

    private User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7),
            RefreshTokenIssuedAt = DateTime.UtcNow
        };
    }
}