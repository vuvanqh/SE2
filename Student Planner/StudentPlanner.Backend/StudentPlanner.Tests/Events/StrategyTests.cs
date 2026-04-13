using Moq;
using StudentPlanner.Core;
using StudentPlanner.Core.Application.EventRequests.Strategies;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Tests.Events;

public class StrategyTests
{
    private readonly Mock<IAcademicEventRepository> _academicEventRepoMock;
    private readonly IAcademicEventRepository _academicEventRepo;

    public StrategyTests()
    {
        _academicEventRepoMock = new Mock<IAcademicEventRepository>();
        _academicEventRepo = _academicEventRepoMock.Object;
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldCreateNewEventAndSave()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "New Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room 101",
                Description = "Test Description"
            },
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending
        };

        AcademicEvent? capturedEvent = null;
        _academicEventRepoMock.Setup(r => r.AddAsync(It.IsAny<AcademicEvent>()))
            .Callback<AcademicEvent>(e => capturedEvent = e)
            .Returns(Task.CompletedTask);


        await strategy.ExecuteAsync(eventRequest);

        Assert.NotNull(capturedEvent);
        Assert.Equal(eventRequest.FacultyId, capturedEvent.FacultyId);
        Assert.Equal(eventRequest.EventDetails.Title, capturedEvent.EventDetails.Title);
        Assert.Equal(capturedEvent.Id, eventRequest.EventId);
        _academicEventRepoMock.Verify(r => r.AddAsync(It.IsAny<AcademicEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateApprovalStrategy_ExecuteAsync_ShouldUpdateExistingEventAndSave()
    {
        var strategy = new UpdateApprovalStrategy(_academicEventRepo);
        var eventId = Guid.NewGuid();
        var existingEvent = new AcademicEvent
        {
            Id = eventId,
            FacultyId = Guid.NewGuid(),
            EventDetails = new EventDetails { Title = "Old Title", Location = "Old Location", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) }
        };

        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            EventId = eventId,
            EventDetails = new EventDetails
            {
                Title = "New Title",
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3),
                Location = "New Location",
                Description = "New Description"
            },
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending
        };

        _academicEventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        _academicEventRepoMock.Setup(r => r.UpdateAsync(It.IsAny<AcademicEvent>()))
            .Returns(Task.CompletedTask);

        await strategy.ExecuteAsync(eventRequest);

        Assert.Equal(eventRequest.EventDetails.Title, existingEvent.EventDetails.Title);
        Assert.Equal(eventRequest.FacultyId, existingEvent.FacultyId);
        _academicEventRepoMock.Verify(r => r.UpdateAsync(It.Is<AcademicEvent>(
            e => e.Id == eventId &&
            e.EventDetails.Title == "New Title")), Times.Once);
    }

    [Fact]
    public async Task DeleteApprovalStrategy_ExecuteAsync_ShouldDeleteExistingEventAndSave()
    {
        var strategy = new DeleteApprovalStrategy(_academicEventRepo);
        var eventId = Guid.NewGuid();
        var existingEvent = new AcademicEvent
        {
            Id = eventId,
            FacultyId = Guid.NewGuid(),
            EventDetails = new EventDetails { Title = "Title" }
        };

        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            EventId = eventId,
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending,
            EventDetails = new EventDetails { Title = "Title" }
        };

        _academicEventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        _academicEventRepoMock.Setup(r => r.DeleteAsync(eventId))
            .Returns(Task.CompletedTask);

        await strategy.ExecuteAsync(eventRequest);

        _academicEventRepoMock.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenTitleIsEmpty()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "", // invalid
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            },
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending
        };

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenEndTimeIsBeforeStartTime()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            EventDetails = new EventDetails
            {
                Title = "Title",
                Location = "Location",
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(1) // end before start
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task UpdateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenTitleIsEmpty()
    {
        var strategy = new UpdateApprovalStrategy(_academicEventRepo);
        var eventId = Guid.NewGuid();
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending,
            EventId = eventId,
            EventDetails = new EventDetails
            {
                Title = "",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            }
        };

        _academicEventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync(new AcademicEvent
            {
                Id = eventId,
                FacultyId = Guid.NewGuid(),
                EventDetails = new EventDetails { Title = "Old", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Location = "Old" }
            });

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task UpdateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenEventNotFound()
    {
        var strategy = new UpdateApprovalStrategy(_academicEventRepo);
        var eventId = Guid.NewGuid();
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending,
            EventId = eventId,
            EventDetails = new EventDetails { Title = "Title", Location = "Loc", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) }
        };

        _academicEventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((AcademicEvent?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task DeleteApprovalStrategy_ExecuteAsync_ShouldNotThrow_WhenEventNotFound()
    {
        var strategy = new DeleteApprovalStrategy(_academicEventRepo);
        var eventId = Guid.NewGuid();
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending,
            EventId = eventId,
            EventDetails = new EventDetails { Title = "Title" }
        };

        _academicEventRepoMock.Setup(r => r.GetByIdAsync(eventId))
            .ReturnsAsync((AcademicEvent?)null);

        var exception = await Record.ExceptionAsync(() => strategy.ExecuteAsync(eventRequest));
        Assert.Null(exception);
        _academicEventRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenEventIdMissing()
    {
        var strategy = new UpdateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending,
            EventId = null, // missing EventId
            EventDetails = new EventDetails { Title = "Title", Location = "Loc", StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task DeleteApprovalStrategy_ExecuteAsync_ShouldThrow_WhenEventIdMissing()
    {
        var strategy = new DeleteApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = RequestStatus.Pending,
            RequestType = RequestType.Delete,
            EventId = null, // Missing EventId
            EventDetails = new EventDetails { Title = "Title" }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenLocationIsMissing()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            EventDetails = new EventDetails
            {
                Title = "Title",
                Location = "",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenStartTimeIsMissing()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            EventDetails = new EventDetails
            {
                Title = "Title",
                Location = "Staff",
                StartTime = default,
                EndTime = DateTime.UtcNow.AddHours(1)
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }

    [Fact]
    public async Task CreateApprovalStrategy_ExecuteAsync_ShouldThrow_WhenEndTimeIsMissing()
    {
        var strategy = new CreateApprovalStrategy(_academicEventRepo);
        var eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            EventDetails = new EventDetails
            {
                Title = "Title",
                Location = "Staff",
                StartTime = DateTime.UtcNow,
                EndTime = default
            }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => strategy.ExecuteAsync(eventRequest));
    }
}
