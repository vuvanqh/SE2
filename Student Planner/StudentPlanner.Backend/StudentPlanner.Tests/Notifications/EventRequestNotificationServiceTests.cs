using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.UI.Hubs;
using StudentPlanner.UI.NotificationServices;
using Xunit;

namespace StudentPlanner.Tests.Notifications;

public class EventRequestNotificationServiceTests
{
    private readonly Mock<IHubContext<EventRequestHub>> _hubContextMock;
    private readonly Mock<IHubClients> _clientsMock;
    private readonly Mock<IClientProxy> _adminProxyMock;
    private readonly Mock<IClientProxy> _userProxyMock;
    private readonly Mock<INotificationPreferenceService> _notificationPreferenceServiceMock;
    private readonly EventRequestNotificationService _service;

    public EventRequestNotificationServiceTests()
    {
        _hubContextMock = new Mock<IHubContext<EventRequestHub>>();
        _clientsMock = new Mock<IHubClients>();
        _adminProxyMock = new Mock<IClientProxy>();
        _userProxyMock = new Mock<IClientProxy>();
        _notificationPreferenceServiceMock = new Mock<INotificationPreferenceService>();

        _hubContextMock.Setup(x => x.Clients).Returns(_clientsMock.Object);
        _clientsMock.Setup(x => x.Group("admins")).Returns(_adminProxyMock.Object);
        _clientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(_userProxyMock.Object);

        _service = new EventRequestNotificationService(
            _hubContextMock.Object,
            _notificationPreferenceServiceMock.Object);
    }

    [Fact]
    public async Task EventRequestUpdated_ShouldNotifyAdminsAndManager_WhenNotificationsAreEnabled()
    {
        var managerId = Guid.NewGuid();

        _notificationPreferenceServiceMock
            .Setup(x => x.AreNotificationsEnabledAsync(managerId))
            .ReturnsAsync(true);

        await _service.EventRequestUpdated(managerId);

        _adminProxyMock.Verify(
            x => x.SendCoreAsync("refreshEventRequests", It.Is<object?[]>(o => o.Length == 0), default),
            Times.Once);

        _userProxyMock.Verify(
            x => x.SendCoreAsync("refreshEventRequests", It.Is<object?[]>(o => o.Length == 0), default),
            Times.Once);
    }

    [Fact]
    public async Task EventRequestUpdated_ShouldNotifyOnlyAdmins_WhenNotificationsAreDisabled()
    {
        var managerId = Guid.NewGuid();

        _notificationPreferenceServiceMock
            .Setup(x => x.AreNotificationsEnabledAsync(managerId))
            .ReturnsAsync(false);

        await _service.EventRequestUpdated(managerId);

        _adminProxyMock.Verify(
            x => x.SendCoreAsync("refreshEventRequests", It.Is<object?[]>(o => o.Length == 0), default),
            Times.Once);

        _userProxyMock.Verify(
            x => x.SendCoreAsync("refreshEventRequests", It.IsAny<object?[]>(), default),
            Times.Never);
    }

    [Fact]
    public async Task NotifyEventRequestListChanged_ShouldNotifyAdmins()
    {
        await _service.NotifyEventRequestListChanged();

        _adminProxyMock.Verify(
            x => x.SendCoreAsync("refreshEventRequests", It.Is<object?[]>(o => o.Length == 0), default),
            Times.Once);
    }
}