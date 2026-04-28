using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using StudentPlanner.Core.Application.AcademicEvents.Services;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core;
using StudentPlanner.Infrastructure.IdentityEntities;

namespace StudentPlanner.Tests.Events;

public class AcademicEventServiceTests
{
    private readonly Mock<IAcademicEventRepository> _academicEventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly AcademicEventService _academicEventService;

    public AcademicEventServiceTests()
    {
        _academicEventRepositoryMock = new Mock<IAcademicEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _academicEventService = new AcademicEventService(
            _academicEventRepositoryMock.Object,
            _userRepositoryMock.Object
        );
    }

    private AcademicEvent GenerateTestEvent(Guid id, Guid facultyId)
    {
        return new AcademicEvent
        {
            Id = id,
            FacultyId = facultyId,
            EventDetails = new EventDetails
            {
                Title = "Test Event",
                Description = "Description",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room 101"
            }
        };
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_ShouldReturnAllEvents()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Student.ToString(),
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC001",
                FacultyCode = "EN",
                FacultyName = "Engineering"
            }
        };

        var events = new List<AcademicEvent>
    {
        GenerateTestEvent(Guid.NewGuid(), facultyId),
        GenerateTestEvent(Guid.NewGuid(), facultyId)
    };

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(repo => repo.GetByFacultyIdAsync(facultyId))
            .ReturnsAsync(events);

        var result = await _academicEventService.GetAccessibleEventsAsync(userId, UserRoleOptions.Student.ToString(), null);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _userRepositoryMock.Verify(
            repo => repo.GetByIdAsync(userId),
            Times.Once);

        _academicEventRepositoryMock.Verify(
            repo => repo.GetByFacultyIdAsync(facultyId),
            Times.Once);
    }


    [Fact]
    public async Task GetAccessibleEventsAsync_ShouldReturnEmptyList_WhenNoEventsExist()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Student.ToString(),
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC001",
                FacultyCode = "EN",
                FacultyName = "Engineering"
            }
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(repo => repo.GetByFacultyIdAsync(facultyId))
            .ReturnsAsync(new List<AcademicEvent>());

        var result = await _academicEventService.GetAccessibleEventsAsync(userId,
             UserRoleOptions.Student.ToString(),
             null);

        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _academicEventRepositoryMock.Verify(
            repo => repo.GetByFacultyIdAsync(facultyId),
            Times.Once);
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_NonAdmin_ReturnsOwnFacultyEvents()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Student.ToString(),
            Email = "student@test.com",
            FirstName = "John",
            LastName = "Doe",
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC",
                FacultyCode = "EN",
                FacultyName = "Engineering"
            }
        };

        var events = new List<AcademicEvent>
    {
        GenerateTestEvent(Guid.NewGuid(), facultyId),
        GenerateTestEvent(Guid.NewGuid(), facultyId)
    };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(x => x.GetByFacultyIdAsync(facultyId))
            .ReturnsAsync(events);

        var result = await _academicEventService.GetAccessibleEventsAsync(
            userId,
            UserRoleOptions.Student.ToString(),
            null
        );

        result.Should().HaveCount(2);

        _academicEventRepositoryMock.Verify(
            x => x.GetByFacultyIdAsync(facultyId),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_NonAdmin_ReturnsEmpty_WhenFacultyHasNoEvents()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Student.ToString(),
            Faculty = new Faculty
            {
                FacultyCode = "Fasfasfsa",
                FacultyName = "fasasassa",
                Id = facultyId,
                FacultyId = "FAC"
            },
            Email = "x",
            FirstName = "x",
            LastName = "x"
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(x => x.GetByFacultyIdAsync(facultyId))
            .ReturnsAsync(new List<AcademicEvent>());

        var result = await _academicEventService.GetAccessibleEventsAsync(
            userId,
            UserRoleOptions.Student.ToString(),
            null
        );

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_AdminWithoutFilters_ReturnsAllEvents()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Admin.ToString(),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var events = new List<AcademicEvent>
    {
        GenerateTestEvent(Guid.NewGuid(), Guid.NewGuid()),
        GenerateTestEvent(Guid.NewGuid(), Guid.NewGuid())
    };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(events);

        var result = await _academicEventService.GetAccessibleEventsAsync(
            userId,
            UserRoleOptions.Admin.ToString(),
            null
        );

        result.Should().HaveCount(2);

        _academicEventRepositoryMock.Verify(
            x => x.GetAllAsync(),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_AdminWithFacultyFilters_ReturnsFilteredEvents()
    {
        var userId = Guid.NewGuid();

        var faculty1 = Guid.NewGuid();
        var faculty2 = Guid.NewGuid();

        var filters = new List<Guid>
    {
        faculty1,
        faculty2
    };

        var user = new User
        {
            Id = userId,
            Role = UserRoleOptions.Admin.ToString(),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User"
        };

        var events = new List<AcademicEvent>
    {
        GenerateTestEvent(Guid.NewGuid(), faculty1),
        GenerateTestEvent(Guid.NewGuid(), faculty2)
    };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _academicEventRepositoryMock
            .Setup(x => x.GetByFacultiesAsync(filters))
            .ReturnsAsync(events);

        var result = await _academicEventService.GetAccessibleEventsAsync(
            userId,
            UserRoleOptions.Admin.ToString(),
            filters
        );

        result.Should().HaveCount(2);

        _academicEventRepositoryMock.Verify(
            x => x.GetByFacultiesAsync(filters),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccessibleEventsAsync_Throws_WhenUserMissing()
    {
        var userId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        Func<Task> act = async () =>
            await _academicEventService.GetAccessibleEventsAsync(
                userId,
                UserRoleOptions.Admin.ToString(),
                null
            );

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnEvent_WhenAdmin()
    {
        var expectedId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedEvent = GenerateTestEvent(expectedId, Guid.NewGuid());
        var user = new User { Id = userId, Role = UserRoleOptions.Admin.ToString(), Email = "admin", FirstName = "A", LastName = "B" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(expectedId)).ReturnsAsync(expectedEvent);

        var result = await _academicEventService.GetEventByIdAsync(expectedId, userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedId);
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnEvent_WhenSameFaculty()
    {
        var expectedId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var expectedEvent = GenerateTestEvent(expectedId, facultyId);
        var user = new User { Id = userId, Role = UserRoleOptions.Student.ToString(), Faculty = new Faculty { Id = facultyId, FacultyId = "F", FacultyCode = "F", FacultyName = "F" }, Email = "s", FirstName = "A", LastName = "B" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(expectedId)).ReturnsAsync(expectedEvent);

        var result = await _academicEventService.GetEventByIdAsync(expectedId, userId);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnNull_WhenDifferentFaculty()
    {
        var expectedId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expectedEvent = GenerateTestEvent(expectedId, Guid.NewGuid()); // Faculty A
        var user = new User { Id = userId, Role = UserRoleOptions.Student.ToString(), Faculty = new Faculty { Id = Guid.NewGuid(), FacultyId = "F", FacultyCode = "F", FacultyName = "F" }, Email = "s", FirstName = "A", LastName = "B" }; // Faculty B

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(expectedId)).ReturnsAsync(expectedEvent);

        var result = await _academicEventService.GetEventByIdAsync(expectedId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEventByIdAsync_ShouldReturnNull_WhenEventDoesNotExist()
    {
        var nonExistentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Role = UserRoleOptions.Student.ToString(), Email = "s", FirstName = "A", LastName = "B" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(nonExistentId)).ReturnsAsync((AcademicEvent?)null);

        var result = await _academicEventService.GetEventByIdAsync(nonExistentId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEventsForUserAsync_ShouldReturnEvents_WhenUserHasFaculty()
    {
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = "Student",
            Faculty = new Faculty { Id = facultyId, FacultyId = "FAC001", FacultyName = "Engineering", FacultyCode = "EN" }
        };

        var events = new List<AcademicEvent>
        {
            GenerateTestEvent(Guid.NewGuid(), facultyId)
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByFacultyIdAsync(facultyId)).ReturnsAsync(events);

        var result = await _academicEventService.GetEventsForUserAsync(userId);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FacultyId.Should().Be(facultyId);
    }

    [Fact]
    public async Task GetEventsForUserAsync_ShouldThrowException_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        Func<Task> act = async () => await _academicEventService.GetEventsForUserAsync(userId);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("User not found.");
    }

    [Fact]
    public async Task GetEventsForUserAsync_ShouldReturnEmpty_WhenUserHasNoFaculty()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "admin@pw.edu.pl",
            FirstName = "Admin",
            LastName = "Doe",
            Role = "Admin"
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _academicEventService.GetEventsForUserAsync(userId);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _academicEventRepositoryMock.Verify(repo => repo.GetByFacultyIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldSubscribe_WhenUserCanAccessEvent()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var academicEvent = GenerateTestEvent(eventId, facultyId);
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRoleOptions.Student.ToString(),
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC001",
                FacultyName = "Engineering",
                FacultyCode = "EN"
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(academicEvent);

        await _academicEventService.SubscribeAsync(eventId, userId);

        _academicEventRepositoryMock.Verify(repo => repo.SubscribeAsync(eventId, userId), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldThrow_WhenEventDoesNotExist()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRoleOptions.Student.ToString()
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync((AcademicEvent?)null);

        Func<Task> act = async () => await _academicEventService.SubscribeAsync(eventId, userId);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Event not found.");
        _academicEventRepositoryMock.Verify(repo => repo.SubscribeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldThrow_WhenUserIsFromDifferentFaculty()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var academicEvent = GenerateTestEvent(eventId, Guid.NewGuid());
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRoleOptions.Student.ToString(),
            Faculty = new Faculty
            {
                Id = Guid.NewGuid(),
                FacultyId = "FAC001",
                FacultyName = "Engineering",
                FacultyCode = "EN"
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(academicEvent);

        Func<Task> act = async () => await _academicEventService.SubscribeAsync(eventId, userId);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Event not found.");
        _academicEventRepositoryMock.Verify(repo => repo.SubscribeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldUnsubscribe_WhenUserCanAccessEvent()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var academicEvent = GenerateTestEvent(eventId, facultyId);
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRoleOptions.Student.ToString(),
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC001",
                FacultyName = "Engineering",
                FacultyCode = "EN"
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(academicEvent);
        _academicEventRepositoryMock
            .Setup(repo => repo.IsSubscribedAsync(eventId, userId))
            .ReturnsAsync(true);
        _academicEventRepositoryMock.Setup(repo => repo.UnsubscribeAsync(eventId, userId)).Returns(Task.CompletedTask);

        await _academicEventService.UnsubscribeAsync(eventId, userId);

        _academicEventRepositoryMock.Verify(repo => repo.UnsubscribeAsync(eventId, userId), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldThrow_WhenSubscriptionDoesNotExist()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facultyId = Guid.NewGuid();

        var academicEvent = GenerateTestEvent(eventId, facultyId);
        var user = new User
        {
            Id = userId,
            Email = "student@pw.edu.pl",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRoleOptions.Student.ToString(),
            Faculty = new Faculty
            {
                Id = facultyId,
                FacultyId = "FAC001",
                FacultyName = "Engineering",
                FacultyCode = "EN"
            }
        };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _academicEventRepositoryMock.Setup(repo => repo.GetByIdAsync(eventId)).ReturnsAsync(academicEvent);
        _academicEventRepositoryMock
            .Setup(repo => repo.IsSubscribedAsync(eventId, userId))
            .ReturnsAsync(false);
        _academicEventRepositoryMock.Setup(repo => repo.UnsubscribeAsync(eventId, userId)).Returns(Task.CompletedTask);

        Func<Task> act = async () => await _academicEventService.UnsubscribeAsync(eventId, userId);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Subscription not found.");
    }
}
