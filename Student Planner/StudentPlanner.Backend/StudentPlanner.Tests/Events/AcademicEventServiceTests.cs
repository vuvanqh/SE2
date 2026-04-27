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
}
