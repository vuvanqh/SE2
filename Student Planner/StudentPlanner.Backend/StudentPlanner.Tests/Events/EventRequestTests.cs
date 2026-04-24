using Moq;
using StudentPlanner.Core;
using StudentPlanner.Core.Application.EventRequests;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.EventRequests.Strategies;
using StudentPlanner.UI.NotificationServices;
using StudentPlanner.UI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace StudentPlanner.Tests;

public class EventRequestTests
{
    private readonly Mock<IEventRequestRepository> _eventRequestRepoMock;
    private readonly IEventRequestRepository _eventRequestRepo;
    private readonly Mock<IEventRequestApprovalStrategy> _createStrategyMock;
    private readonly Mock<IEventRequestApprovalStrategy> _updateStrategyMock;
    private readonly Mock<IEventRequestApprovalStrategy> _deleteStrategyMock;
    private readonly Mock<IHubContext<EventRequestHub>> _erHubMock;

    public EventRequestTests()
    {
        _eventRequestRepoMock = new Mock<IEventRequestRepository>();
        _eventRequestRepo = _eventRequestRepoMock.Object;
        _createStrategyMock = new Mock<IEventRequestApprovalStrategy>();
        _createStrategyMock.Setup(s => s.RequestType).Returns(RequestType.Create);
        _updateStrategyMock = new Mock<IEventRequestApprovalStrategy>();
        _updateStrategyMock.Setup(s => s.RequestType).Returns(RequestType.Update);
        _deleteStrategyMock = new Mock<IEventRequestApprovalStrategy>();
        _deleteStrategyMock.Setup(s => s.RequestType).Returns(RequestType.Delete);

        _erHubMock = new Mock<IHubContext<EventRequestHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
        _erHubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
    }

    private EventRequestService CreateService()
    {
        return new EventRequestService(
            _eventRequestRepo,
            new List<IEventRequestApprovalStrategy>
            {
                _createStrategyMock.Object,
                _updateStrategyMock.Object,
                _deleteStrategyMock.Object
            }, new EventRequestNotificationService(_erHubMock.Object)
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRequest_WhenCreateRequestIsValid()
    {
        EventRequest? result = null;
        Guid managerId = Guid.NewGuid();

        CreateEventRequestRequest request = new CreateEventRequestRequest
        {
            FacultyId = Guid.NewGuid(),
            EventId = null,
            RequestType = RequestType.Create,
            EventDetails = new EventDetailsDto
            {
                Title = "New Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            }
        };

        _eventRequestRepoMock.Setup(r => r.AddAsync(It.IsAny<EventRequest>()))
            .Callback<EventRequest>(e => result = e)
            .Returns(Task.CompletedTask);

        EventRequestService service = CreateService();

        Guid createdId = await service.CreateAsync(managerId, request);

        Assert.NotNull(result);
        Assert.Equal(createdId, result!.Id);
        Assert.Equal(managerId, result.ManagerId);
        Assert.Equal(RequestStatus.Pending, result.Status);
        Assert.Equal(request.RequestType, result.RequestType);
        Assert.Equal(request.FacultyId, result.FacultyId);
        Assert.Equal(request.EventId, result.EventId);
        Assert.Null(result.EventId);
        Assert.Equal(request.EventDetails.Title, result.EventDetails.Title);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCreateRequestContainsEventId()
    {
        Guid managerId = Guid.NewGuid();

        CreateEventRequestRequest request = new CreateEventRequestRequest
        {
            FacultyId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            RequestType = RequestType.Create,
            EventDetails = new EventDetailsDto
            {
                Title = "New Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            }
        };

        EventRequestService service = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(managerId, request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUpdateRequestDoesNotContainEventId()
    {
        Guid managerId = Guid.NewGuid();

        CreateEventRequestRequest request = new CreateEventRequestRequest
        {
            FacultyId = Guid.NewGuid(),
            EventId = null,
            RequestType = RequestType.Update,
            EventDetails = new EventDetailsDto
            {
                Title = "Updated Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            }
        };

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(managerId, request));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRequest_WhenRequestIsPendingAndOwnedByManager()
    {
        Guid managerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = managerId,
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await service.DeleteAsync(managerId, requestId);

        _eventRequestRepoMock.Verify(r => r.DeleteAsync(requestId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid managerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteAsync(managerId, requestId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenManagerDoesNotOwnRequest()
    {
        Guid managerId = Guid.NewGuid();
        Guid otherManagerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = otherManagerId,
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(managerId, requestId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenRequestIsNotPending()
    {
        Guid managerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = managerId,
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Approved
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(managerId, requestId));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid requestId = Guid.NewGuid();

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByIdAsync(requestId));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRequest_WhenRequestExists()
    {
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        EventRequestResponse? result = await service.GetByIdAsync(requestId);

        Assert.NotNull(result);
        Assert.Equal(requestId, result!.Id);
    }

    [Fact]
    public async Task GetByManagerIdAsync_ShouldReturnOnlyManagerRequests()
    {
        Guid managerId = Guid.NewGuid();

        List<EventRequest> requests = new()
        {
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = managerId,
                ReviewedByAdminId = null,
                EventId = null,
                EventDetails = new EventDetails
                {
                    Title = "Create Event",
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Location = "Room A",
                    Description = "Description"
                },
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = null,
                RequestType = RequestType.Create,
                Status = RequestStatus.Pending
            },
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = managerId,
                ReviewedByAdminId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventDetails = new EventDetails
                {
                    Title = "Update Event",
                    StartTime = DateTime.UtcNow.AddHours(3),
                    EndTime = DateTime.UtcNow.AddHours(4),
                    Location = "Room B",
                    Description = "Description"
                },
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                RequestType = RequestType.Update,
                Status = RequestStatus.Rejected
            }
        };

        _eventRequestRepoMock.Setup(r => r.GetByManagerIdAsync(managerId))
            .ReturnsAsync(requests);

        EventRequestService service = CreateService();

        List<EventRequestResponse> result = await service.GetByManagerIdAsync(managerId);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(managerId, r.ManagerId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRequests()
    {
        List<EventRequest> requests = new()
        {
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                ReviewedByAdminId = null,
                EventId = null,
                EventDetails = new EventDetails
                {
                    Title = "Create Event",
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Location = "Room A",
                    Description = "Description"
                },
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = null,
                RequestType = RequestType.Create,
                Status = RequestStatus.Pending
            },
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                ReviewedByAdminId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                EventDetails = new EventDetails
                {
                    Title = "Delete Event",
                    StartTime = DateTime.UtcNow.AddHours(3),
                    EndTime = DateTime.UtcNow.AddHours(4),
                    Location = "Room B",
                    Description = "Description"
                },
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                RequestType = RequestType.Delete,
                Status = RequestStatus.Approved
            }
        };

        _eventRequestRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        EventRequestService service = CreateService();

        List<EventRequestResponse> result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApproveAsync_ShouldApproveCreateRequest_WhenRequestIsPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();
        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = null,
            EventDetails = new EventDetails { Title = "New Event" },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);
        _createStrategyMock.Setup(s => s.ExecuteAsync(eventRequest))
            .Returns(Task.CompletedTask);

        EventRequestService service = CreateService();

        await service.ApproveAsync(adminId, requestId);

        Assert.Equal(RequestStatus.Approved, eventRequest.Status);
        Assert.Equal(adminId, eventRequest.ReviewedByAdminId);
        _createStrategyMock.Verify(s => s.ExecuteAsync(eventRequest), Times.Once);
        _eventRequestRepoMock.Verify(r => r.UpdateAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldApproveUpdateRequest_WhenRequestIsPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = null,
            EventDetails = new EventDetails { Title = "Update Event" },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending
        };
        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
           .ReturnsAsync(eventRequest);
        _updateStrategyMock.Setup(s => s.ExecuteAsync(eventRequest))
            .Returns(Task.CompletedTask);

        EventRequestService service = CreateService();

        await service.ApproveAsync(adminId, requestId);

        Assert.Equal(RequestStatus.Approved, eventRequest.Status);
        _updateStrategyMock.Verify(s => s.ExecuteAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldApproveDeleteRequest_WhenRequestIsPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = null,
            EventDetails = new EventDetails { Title = "Delete Event" },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);
        _deleteStrategyMock.Setup(s => s.ExecuteAsync(eventRequest))
            .Returns(Task.CompletedTask);

        EventRequestService service = CreateService();

        await service.ApproveAsync(adminId, requestId);

        Assert.Equal(RequestStatus.Approved, eventRequest.Status);
        _deleteStrategyMock.Verify(s => s.ExecuteAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.ApproveAsync(adminId, requestId));
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenRequestIsNotPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Update Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Update,
            Status = RequestStatus.Rejected
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveAsync(adminId, requestId));
    }

    [Fact]
    public async Task RejectAsync_ShouldRejectRequest_WhenRequestIsPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await service.RejectAsync(adminId, requestId);

        Assert.Equal(RequestStatus.Rejected, eventRequest.Status);
        Assert.Equal(adminId, eventRequest.ReviewedByAdminId);
        Assert.NotNull(eventRequest.ReviewedAt);

        _eventRequestRepoMock.Verify(r => r.UpdateAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.RejectAsync(adminId, requestId));
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenRequestIsNotPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            EventId = Guid.NewGuid(),
            EventDetails = new EventDetails
            {
                Title = "Delete Event",
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room A",
                Description = "Description"
            },
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = RequestType.Delete,
            Status = RequestStatus.Approved
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RejectAsync(adminId, requestId));
    }
}
