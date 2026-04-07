using Moq;
using StudentPlanner.Core.Application.EventRequests;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Tests;

public class EventRequestTests
{
    private readonly Mock<IEventRequestRepository> _eventRequestRepoMock;
    private readonly IEventRequestRepository _eventRequestRepo;

    public EventRequestTests()
    {
        _eventRequestRepoMock = new Mock<IEventRequestRepository>();
        _eventRequestRepo = _eventRequestRepoMock.Object;
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRequest_WhenRequestIsValid()
    {
        EventRequest? result = null;
        Guid managerId = Guid.NewGuid();

        CreateEventRequestRequest request = new CreateEventRequestRequest
        {
            FacultyId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            RequestType = RequestType.Create
        };

        _eventRequestRepoMock.Setup(r => r.AddAsync(It.IsAny<EventRequest>()))
            .Callback<EventRequest>(e => result = e)
            .Returns(Task.CompletedTask);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        Guid createdId = await service.CreateAsync(managerId, request);

        Assert.NotNull(result);
        Assert.Equal(createdId, result!.Id);
        Assert.Equal(managerId, result.ManagerId);
        Assert.Equal(RequestStatus.Pending, result.Status);
        Assert.Equal(request.RequestType, result.RequestType);
        Assert.Equal(request.FacultyId, result.FacultyId);
        Assert.Equal(request.EventId, result.EventId);
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
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid(),
            //Event = null!
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

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

        EventRequestService service = new EventRequestService(_eventRequestRepo);

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
            RequestType = RequestType.Create,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

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
            RequestType = RequestType.Create,
            Status = RequestStatus.Approved,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(managerId, requestId));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRequest_WhenRequestExistsAndBelongsToManager()
    {
        Guid managerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = managerId,
            ReviewedByAdminId = null,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        EventRequestResponse? result = await service.GetByIdAsync(managerId, requestId);

        Assert.NotNull(result);
        Assert.Equal(requestId, result!.Id);
        Assert.Equal(managerId, result.ManagerId);
        Assert.Equal(RequestType.Update, result.RequestType);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid managerId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetByIdAsync(managerId, requestId));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenRequestBelongsToAnotherManager()
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
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByIdAsync(managerId, requestId));
    }

    [Fact]
    public async Task GetByManagerIdAsync_ShouldReturnOnlyManagerRequests()
    {
        Guid managerId = Guid.NewGuid();

        List<EventRequest> requests = new List<EventRequest>
        {
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = managerId,
                ReviewedByAdminId = null,
                RequestType = RequestType.Create,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = null,
                ReviewComment = null,
                EventId = Guid.NewGuid()
            },
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = managerId,
                ReviewedByAdminId = null,
                RequestType = RequestType.Update,
                Status = RequestStatus.Rejected,
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                ReviewComment = "Rejected",
                EventId = Guid.NewGuid()
            }
        };

        _eventRequestRepoMock.Setup(r => r.GetByManagerIdAsync(managerId))
            .ReturnsAsync(requests);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        List<EventRequestResponse> result = await service.GetByManagerIdAsync(managerId);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(managerId, r.ManagerId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRequests()
    {
        List<EventRequest> requests = new List<EventRequest>
        {
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                ReviewedByAdminId = null,
                RequestType = RequestType.Create,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = null,
                ReviewComment = null,
                EventId = Guid.NewGuid()
            },
            new EventRequest
            {
                Id = Guid.NewGuid(),
                FacultyId = Guid.NewGuid(),
                ManagerId = Guid.NewGuid(),
                ReviewedByAdminId = Guid.NewGuid(),
                RequestType = RequestType.Delete,
                Status = RequestStatus.Approved,
                CreatedAt = DateTime.UtcNow,
                ReviewedAt = DateTime.UtcNow,
                ReviewComment = "Approved",
                EventId = Guid.NewGuid()
            }
        };

        _eventRequestRepoMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        List<EventRequestResponse> result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ApproveAsync_ShouldApproveRequest_WhenRequestIsPending()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        EventRequest eventRequest = new EventRequest
        {
            Id = requestId,
            FacultyId = Guid.NewGuid(),
            ManagerId = Guid.NewGuid(),
            ReviewedByAdminId = null,
            RequestType = RequestType.Update,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Approved by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await service.ApproveAsync(adminId, requestId, request);

        Assert.Equal(RequestStatus.Approved, eventRequest.Status);
        Assert.Equal(adminId, eventRequest.ReviewedByAdminId);
        Assert.NotNull(eventRequest.ReviewedAt);
        Assert.Equal("Approved by admin", eventRequest.ReviewComment);

        _eventRequestRepoMock.Verify(r => r.UpdateAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task ApproveAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Approved by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => service.ApproveAsync(adminId, requestId, request));
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
            RequestType = RequestType.Update,
            Status = RequestStatus.Rejected,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Approved by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveAsync(adminId, requestId, request));
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
            RequestType = RequestType.Delete,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Rejected by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await service.RejectAsync(adminId, requestId, request);

        Assert.Equal(RequestStatus.Rejected, eventRequest.Status);
        Assert.Equal(adminId, eventRequest.ReviewedByAdminId);
        Assert.NotNull(eventRequest.ReviewedAt);
        Assert.Equal("Rejected by admin", eventRequest.ReviewComment);

        _eventRequestRepoMock.Verify(r => r.UpdateAsync(eventRequest), Times.Once);
    }

    [Fact]
    public async Task RejectAsync_ShouldThrow_WhenRequestDoesNotExist()
    {
        Guid adminId = Guid.NewGuid();
        Guid requestId = Guid.NewGuid();

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Rejected by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((EventRequest?)null);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RejectAsync(adminId, requestId, request));
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
            RequestType = RequestType.Delete,
            Status = RequestStatus.Approved,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            ReviewComment = null,
            EventId = Guid.NewGuid()
        };

        ReviewEventRequestRequest request = new ReviewEventRequestRequest
        {
            ReviewComment = "Rejected by admin"
        };

        _eventRequestRepoMock.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(eventRequest);

        EventRequestService service = new EventRequestService(_eventRequestRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RejectAsync(adminId, requestId, request));
    }
}
