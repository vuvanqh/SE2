using FluentAssertions;
using Moq;
using StudentPlanner.Core.Application.Notifications.DTOs;
using StudentPlanner.Core.Application.Notifications.Services;
using StudentPlanner.Core.Domain.RepositoryContracts;
using Xunit;

namespace StudentPlanner.Tests.Notifications;

public class NotificationPreferenceServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly NotificationPreferenceService _service;

    public NotificationPreferenceServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new NotificationPreferenceService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetPreferenceAsync_ShouldReturnPreference_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetNotificationPreferenceAsync(userId)).ReturnsAsync(true);

        var result = await _service.GetPreferenceAsync(userId);

        result.NotificationsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetPreferenceAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetNotificationPreferenceAsync(userId)).ReturnsAsync((bool?)null);

        Func<Task> act = async () => await _service.GetPreferenceAsync(userId);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdatePreferenceAsync_ShouldPersistPreference()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateNotificationPreferenceRequest { NotificationsEnabled = false };

        await _service.UpdatePreferenceAsync(userId, request);

        _userRepositoryMock.Verify(x => x.UpdateNotificationPreferenceAsync(userId, false), Times.Once);
    }

    [Fact]
    public async Task AreNotificationsEnabledAsync_ShouldReturnFalse_WhenDisabled()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetNotificationPreferenceAsync(userId)).ReturnsAsync(false);

        var result = await _service.AreNotificationsEnabledAsync(userId);

        result.Should().BeFalse();
    }
}
