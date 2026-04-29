using Moq;
using StudentPlanner.Core.Application.Events;
using StudentPlanner.Core.Application.Events.EventPreveiws;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace StudentPlanner.Tests.Events;

public class EventPreviewTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPersonalEventRepository> _personalEventRepoMock;
    private readonly Mock<IAcademicEventRepository> _academicEventRepoMock;
    private readonly Mock<IUsosClient> _usosClientMock;
    private readonly Mock<IMemoryCache> _cacheMock;

    public EventPreviewTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _personalEventRepoMock = new Mock<IPersonalEventRepository>();
        _academicEventRepoMock = new Mock<IAcademicEventRepository>();
        _usosClientMock = new Mock<IUsosClient>();
        _cacheMock = new Mock<IMemoryCache>();
    }

    #region EventPreviewService Tests

    [Fact]
    public async Task EventPreviewService_GetForUserAsync_ShouldCallStrategiesAndAggregateResults()
    {
        var userContext = new UserContext { Id = Guid.NewGuid(), Role = UserRoleOptions.Student };
        var query = new EventPreviewQuery { From = DateTime.UtcNow, Days = 7 };
        
        var strategy1Mock = new Mock<IEventPreviewStrategy>();
        strategy1Mock.Setup(s => s.CanHandle(userContext)).Returns(true);
        strategy1Mock.Setup(s => s.GetAsync(userContext, query))
            .ReturnsAsync(new List<EventPreveiwDto> { new EventPreveiwDto { Id = "1", Title = "Event 1" } });

        var strategy2Mock = new Mock<IEventPreviewStrategy>();
        strategy2Mock.Setup(s => s.CanHandle(userContext)).Returns(true);
        strategy2Mock.Setup(s => s.GetAsync(userContext, query))
            .ReturnsAsync(new List<EventPreveiwDto> { new EventPreveiwDto { Id = "2", Title = "Event 2" } });

        var service = new EventPreviewService(new[] { strategy1Mock.Object, strategy2Mock.Object }, _userRepoMock.Object);

        var result = await service.GetForUserAsync(userContext, query);

        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Id == "1");
        result.Should().Contain(e => e.Id == "2");
        _userRepoMock.Verify(r => r.GetByIdAsync(userContext.Id), Times.Once);
    }

    #endregion

    #region PersonalEventPreveiwStrategy Tests

    [Fact]
    public async Task PersonalEventPreveiwStrategy_GetAsync_ShouldReturnFilteredEvents()
    {
        var userId = Guid.NewGuid();
        var userContext = new UserContext { Id = userId, Role = UserRoleOptions.Student };
        var from = new DateTime(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var query = new EventPreviewQuery { From = from, Days = 7 };
        
        var events = new List<PersonalEvent>
        {
            new PersonalEvent { Id = Guid.NewGuid(), UserId = userId, EventDetails = new EventDetails { Title = "In Range", StartTime = from.AddDays(1), EndTime = from.AddDays(1).AddHours(1) } },
            new PersonalEvent { Id = Guid.NewGuid(), UserId = userId, EventDetails = new EventDetails { Title = "Out of Range", StartTime = from.AddDays(10), EndTime = from.AddDays(10).AddHours(1) } }
        };

        _personalEventRepoMock.Setup(r => r.GetEventsByUserIdAsync(userId)).ReturnsAsync(events);
        var strategy = new PersonalEventPreveiwStrategy(_personalEventRepoMock.Object);

        var result = await strategy.GetAsync(userContext, query);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("In Range");
        result.First().EventType.Should().Be(EventPreveiwType.PersonalEvent);
    }

    #endregion

    #region AcademicEventPreviewStrategy Tests

    [Fact]
    public async Task AcademicEventPreviewStrategy_GetAsync_Admin_ShouldReturnAllEvents()
    {
        var userContext = new UserContext { Id = Guid.NewGuid(), Role = UserRoleOptions.Admin };
        var query = new EventPreviewQuery();
        
        var events = new List<AcademicEvent>
        {
            new FacultyEvent { Id = Guid.NewGuid(), EventDetails = new EventDetails { Title = "Admin Event", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) } }
        };

        _academicEventRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(events);
        var strategy = new AcademicEventPreviewStrategy(_academicEventRepoMock.Object);

        var result = await strategy.GetAsync(userContext, query);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Admin Event");
    }

    [Fact]
    public async Task AcademicEventPreviewStrategy_GetAsync_Student_ShouldReturnSubscribedEvents()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var userContext = new UserContext { Id = userId, Role = UserRoleOptions.Student, FacultyId = facultyId };
        var query = new EventPreviewQuery();
        
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var events = new List<AcademicEvent>
        {
            new FacultyEvent { Id = eventId1, FacultyId = facultyId, EventDetails = new EventDetails { Title = "Subscribed", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) } },
            new FacultyEvent { Id = eventId2, FacultyId = facultyId, EventDetails = new EventDetails { Title = "Not Subscribed", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) } }
        };

        _academicEventRepoMock.Setup(r => r.GetUniversityEventsAsync()).ReturnsAsync(new List<AcademicEvent>());
        _academicEventRepoMock.Setup(r => r.GetByFacultyIdAsync(facultyId)).ReturnsAsync(events);
        _academicEventRepoMock.Setup(r => r.GetSubscribedEventIdsAsync(userId)).ReturnsAsync(new HashSet<Guid> { eventId1 });
        
        var strategy = new AcademicEventPreviewStrategy(_academicEventRepoMock.Object);

        var result = await strategy.GetAsync(userContext, query);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Subscribed");
        result.First().EventType.Should().Be(EventPreveiwType.AcademicEvent);
    }

    #endregion

    #region UsosEventPreviewStrategy Tests

    [Fact]
    public async Task UsosEventPreviewStrategy_GetAsync_ShouldReturnUsosEvents()
    {
        var userId = Guid.NewGuid();
        var userContext = new UserContext { Id = userId, Role = UserRoleOptions.Student };
        var from = DateTime.UtcNow.Date;
        var query = new EventPreviewQuery { From = from, Days = 7 };
        var userEntity = new User 
        { 
            Id = userId, 
            UsosToken = "valid-token",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Role = UserRoleOptions.Student.ToString()
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(userEntity);
        
        var usosEvents = new List<UsosEventResponseDto>
        {
            new UsosEventResponseDto 
            { 
                Id = "usos1", 
                Title = "USOS Event", 
                StartTime = from.AddHours(10).ToString("yyyy-MM-dd HH:mm:ss"), 
                EndTime = from.AddHours(12).ToString("yyyy-MM-dd HH:mm:ss"),
                BuildingName = "Building",
                RoomNumber = "101"
            }
        };

        _usosClientMock.Setup(c => c.GetTimetableAsync("valid-token", DateOnly.FromDateTime(from), 7))
            .ReturnsAsync(usosEvents);

        object? cacheEntry = null;
        _cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);
        _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        var strategy = new UsosEventPreviewStrategy(_cacheMock.Object, _usosClientMock.Object, _userRepoMock.Object);

        var result = await strategy.GetAsync(userContext, query);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("USOS Event");
        result.First().EventType.Should().Be(EventPreveiwType.UsosEvent);
        _usosClientMock.Verify(c => c.GetTimetableAsync("valid-token", DateOnly.FromDateTime(from), 7), Times.Once);
    }

    [Fact]
    public async Task UsosEventPreviewStrategy_GetAsync_ShouldThrowWhenNoToken()
    {
        var userId = Guid.NewGuid();
        var userContext = new UserContext { Id = userId, Role = UserRoleOptions.Student };
        var userEntity = new User 
        { 
            Id = userId, 
            UsosToken = "",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Role = UserRoleOptions.Student.ToString()
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(userEntity);
        
        object? cacheEntry = null;
        _cacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);

        var strategy = new UsosEventPreviewStrategy(_cacheMock.Object, _usosClientMock.Object, _userRepoMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.GetAsync(userContext, new EventPreviewQuery()));
    }

    #endregion
}
