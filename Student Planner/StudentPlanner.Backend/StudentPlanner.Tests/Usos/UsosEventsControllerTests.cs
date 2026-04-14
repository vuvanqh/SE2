using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure.Repositories;
using StudentPlanner.UI.Controllers;
using System.Security.Claims;
using Xunit;
namespace StudentPlanner.Tests.Usos;

public class UsosEventsControllerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUsosClient> _usosClientMock;

    public UsosEventsControllerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _usosClientMock = new Mock<IUsosClient>();
    }

    private static UsosEventsController CreateController(
       IUserRepository userRepository,
       IUsosClient usosClient,
       ClaimsPrincipal? user = null)
    {
        return new UsosEventsController(userRepository, usosClient)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user ?? new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };
    }
    [Fact]
    public async Task GetMyEvents_ShouldReturnUnauthorized_WhenNameIdentifierClaimMissing()
    {
        var controller = CreateController(_userRepositoryMock.Object, _usosClientMock.Object);

        var result = await controller.GetMyEvents("2025-10-01", 7);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }
    [Fact]
    public async Task GetMyEvents_ShouldReturnUnauthorized_WhenNameIdentifierIsInvalidGuid()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") },
                "TestAuth"));

        var controller = CreateController(_userRepositoryMock.Object, _usosClientMock.Object, principal);

        var result = await controller.GetMyEvents("2025-10-01", 7);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }
    [Fact]
    public async Task GetMyEvents_ShouldReturnBadRequest_WhenUserHasNoUsosToken()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = "Student",
            UsosToken = null
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "TestAuth"));

        var controller = CreateController(_userRepositoryMock.Object, _usosClientMock.Object, principal);

        var result = await controller.GetMyEvents("2025-10-01", 7);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
    [Fact]
    public async Task GetMyEvents_ShouldReturnOk_WithFetchedEvents()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            Role = "Student",
            UsosToken = "fresh-usos-token"
        };

        var events = new List<UsosEventResponseDto>
        {
            new()
            {
                Title = "Algorithms - Lab 1",
                StartTime = "2025-10-03 12:15:00",
                EndTime = "2025-10-03 14:00:00",
                CourseId = "101",
                ClassType = "Laboratory",
                GroupNumber = "1",
                BuildingId = "B2",
                BuildingName = "Lab Building",
                RoomNumber = "201",
                RoomId = "R2"
            }
        };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _usosClientMock
            .Setup(c => c.GetTimetableAsync(user.UsosToken!, new DateOnly(2025, 10, 1), 7))
            .ReturnsAsync(events);

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "TestAuth"));

        var controller = CreateController(_userRepositoryMock.Object, _usosClientMock.Object, principal);

        var result = await controller.GetMyEvents("2025-10-01", 7);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeAssignableTo<List<UsosEventResponseDto>>().Subject;

        payload.Should().HaveCount(1);
        payload[0].Title.Should().Be("Algorithms - Lab 1");

        _usosClientMock.Verify(
            c => c.GetTimetableAsync("fresh-usos-token", new DateOnly(2025, 10, 1), 7),
            Times.Once);
    }

}