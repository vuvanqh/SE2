using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentPlanner.Core.Application.Notifications.DTOs;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.UI.Controllers;
using System.Security.Claims;
using Xunit;

namespace StudentPlanner.Tests.Notifications;

public class NotificationPreferencesControllerTests
{
    private readonly Mock<INotificationPreferenceService> _serviceMock;
    private readonly NotificationPreferencesController _controller;

    public NotificationPreferencesControllerTests()
    {
        _serviceMock = new Mock<INotificationPreferenceService>();
        _controller = new NotificationPreferencesController(_serviceMock.Object);
    }

    private void SetAuthenticatedUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetPreference_ShouldReturn200AndPreference_WhenUserIsAuthenticated()
    {
        var userId = Guid.NewGuid();
        SetAuthenticatedUser(userId);

        _serviceMock.Setup(x => x.GetPreferenceAsync(userId))
            .ReturnsAsync(new NotificationPreferenceResponse
            {
                NotificationsEnabled = true
            });

        var result = await _controller.GetPreference();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<NotificationPreferenceResponse>().Subject;
        response.NotificationsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task GetPreference_ShouldReturn401_WhenUserIsUnauthenticated()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await _controller.GetPreference();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdatePreference_ShouldReturn204_WhenRequestIsValid()
    {
        var userId = Guid.NewGuid();
        SetAuthenticatedUser(userId);

        var request = new UpdateNotificationPreferenceRequest
        {
            NotificationsEnabled = false
        };

        var result = await _controller.UpdatePreference(request);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(x => x.UpdatePreferenceAsync(userId, request), Times.Once);
    }

    [Fact]
    public async Task UpdatePreference_ShouldReturn401_WhenUserIsUnauthenticated()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var request = new UpdateNotificationPreferenceRequest
        {
            NotificationsEnabled = false
        };

        var result = await _controller.UpdatePreference(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
        _serviceMock.Verify(x => x.UpdatePreferenceAsync(It.IsAny<Guid>(), It.IsAny<UpdateNotificationPreferenceRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePreference_ShouldReturn404_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        SetAuthenticatedUser(userId);

        var request = new UpdateNotificationPreferenceRequest
        {
            NotificationsEnabled = false
        };

        _serviceMock.Setup(x => x.UpdatePreferenceAsync(userId, request))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var result = await _controller.UpdatePreference(request);

        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().NotBeNull();
    }
}