using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Exceptions;
using StudentPlanner.Infrastructure.Services;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace StudentPlanner.Tests.Usos;

public class UsosClientTests
{
    [Fact]
    public async Task GetTimetableAsync_ShouldSendBearerToken_AndDeserializeResponse()
    {
        var handler = new StubHttpMessageHandler(async request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.ToString().Should().Contain("/services/tt/user?start=2025-10-01&days=7");

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("fresh-usos-token");

            var json = """
            [
              {
                "title": "Algorithms - Lab 1",
                "start_time": "2025-10-03 12:15:00",
                "end_time": "2025-10-03 14:00:00",
                "course_id": "101",
                "class_type": "Laboratory",
                "group_number": "1",
                "building_id": "B2",
                "building_name": "Lab Building",
                "room_number": "201",
                "room_id": "R2"
              }
            ]
            """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var logger = new Mock<ILogger<UsosClient>>();
        var client = new UsosClient(httpClient, logger.Object);

        var result = await client.GetTimetableAsync("fresh-usos-token", new DateOnly(2025, 10, 1), 7);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Algorithms - Lab 1");
        result[0].StartTime.Should().Be("2025-10-03 12:15:00");
        result[0].CourseId.Should().Be("101");
        result[0].ClassType.Should().Be("Laboratory");
    }

    [Fact]
    public async Task GetTimetableAsync_ShouldThrowUsosException_WhenResponseIsUnauthorized()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:3000")
        };

        var logger = new Mock<ILogger<UsosClient>>();
        var client = new UsosClient(httpClient, logger.Object);

        var act = async () => await client.GetTimetableAsync("bad-token", new DateOnly(2025, 10, 1), 7);

        await act.Should()
            .ThrowAsync<UsosException>()
            .WithMessage("*Unauthorized*");
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }
}