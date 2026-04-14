using FluentAssertions;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Moq;
namespace StudentPlanner.Tests.Usos;

public class UsosEventsControllerE2ETests : IntegrationTestBase
{
    public UsosEventsControllerE2ETests(StudentPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMyEvents_ShouldReturn401_WhenNoJwtProvided()
    {
        var response = await _client.GetAsync("/api/usos-events/me?start=2025-10-01&days=7", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyEvents_ShouldReturn200_WithExpectedEvents()
    {
        var email = $"usos-{Guid.NewGuid():N}@pw.edu.pl";
        var password = "Password123!";

        _factory.UsosAuthServiceMock
            .Setup(x => x.GetTimetableAsync("test-token", new DateOnly(2025, 10, 1), 7))
            .ReturnsAsync(new List<UsosEventResponseDto>
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
            });

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
        loginPayload.Should().NotBeNull();
        loginPayload!.Token.Should().NotBeNullOrWhiteSpace();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.Token);

        var response = await _client.GetAsync("/api/usos-events/me?start=2025-10-01&days=7", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<UsosEventResponseDto>>(cancellationToken: TestContext.Current.CancellationToken);
        events.Should().NotBeNull();
        events!.Should().HaveCount(1);
        events[0].Title.Should().Be("Algorithms - Lab 1");
        events[0].CourseId.Should().Be("101");

        _factory.UsosAuthServiceMock.Verify(
            x => x.GetTimetableAsync("test-token", new DateOnly(2025, 10, 1), 7),
            Times.Once);
    }
}