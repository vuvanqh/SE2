using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Events.UsosEvents.Services;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Logging;
using Xunit;

namespace StudentPlanner.Tests.Usos;

public class UsosEventServiceTests
{
    [Fact]
    public async Task SyncAndGetEventsAsync_ShouldReturnCachedEvents_WhenCacheAlreadyContainsData()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var usosClientMock = new Mock<IUsosClient>();

        var cache = new MemoryCache(new MemoryCacheOptions());

        var userId = Guid.NewGuid();
        var start = new DateOnly(2025, 10, 1);
        var days = 7;

        var cachedEvents = new List<UsosEventResponseDto>
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

        var cacheKey = $"usos-events-{userId}-{start:yyyyMMdd}-{days}";
        cache.Set(cacheKey, cachedEvents, TimeSpan.FromMinutes(30));

        var service = new UsosEventService(
            usosClientMock.Object,
            cache,
            userRepositoryMock.Object,
            Mock.Of<ILogger<UsosEventService>>()
        );

        // Act
        var result = await service.SyncAndGetEventsAsync(userId, start, days);

        // Assert
        result.Should().BeSameAs(cachedEvents);

        usosClientMock.Verify(
            x => x.GetTimetableAsync(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<int>()),
            Times.Never);

        userRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task SyncAndGetEventsAsync_ShouldReturnEvents_WhenCacheDoesNotContainData()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var usosClientMock = new Mock<IUsosClient>();

        var cache = new MemoryCache(new MemoryCacheOptions());

        var userId = Guid.NewGuid();
        var start = new DateOnly(2025, 10, 1);
        var days = 7;

        var user = new User
        {
            Id = userId,
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "email@pw.edu.pl",
            Role = "Student",
            UsosToken = "test-token"
        };

        var fetchedEvents = new List<UsosEventResponseDto>
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

        userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        usosClientMock
            .Setup(x => x.GetTimetableAsync("test-token", start, days))
            .ReturnsAsync(fetchedEvents);

        var service = new UsosEventService(
            usosClientMock.Object,
            cache,
            userRepositoryMock.Object,
            Mock.Of<ILogger<UsosEventService>>()
        );

        // Act
        var result = await service.SyncAndGetEventsAsync(userId, start, days);

        // Assert
        result.Should().BeSameAs(fetchedEvents);

        usosClientMock.Verify(
            x => x.GetTimetableAsync("test-token", start, days),
            Times.Once);

        userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);
    }

}