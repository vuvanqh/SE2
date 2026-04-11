using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.EventRequests.Strategies;
using System.Collections.Generic;

namespace StudentPlanner.Core.Application.EventRequests;

public class EventRequestService : IEventRequestService
{
    private readonly IEventRequestRepository _eventRequestRepository;
    private readonly IReadOnlyDictionary<RequestType, IEventRequestApprovalStrategy> _strategies;

    public EventRequestService(
        IEventRequestRepository eventRequestRepository,
        CreateApprovalStrategy createStrategy,
        UpdateApprovalStrategy updateStrategy,
        DeleteApprovalStrategy deleteStrategy)
    {
        _eventRequestRepository = eventRequestRepository;
        _strategies = new Dictionary<RequestType, IEventRequestApprovalStrategy>
        {
            { RequestType.Create, createStrategy },
            { RequestType.Update, updateStrategy },
            { RequestType.Delete, deleteStrategy }
        };
    }

    public async Task<Guid> CreateAsync(Guid managerId, CreateEventRequestRequest request)
    {
        if (request.RequestType == RequestType.Create && request.EventId != null)
        {
            throw new ArgumentException("Create request should not contain EventId.");
        }

        if ((request.RequestType == RequestType.Update || request.RequestType == RequestType.Delete) && request.EventId == null)
        {
            throw new ArgumentException("Update and Delete requests must contain EventId.");
        }
        EventRequest eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = request.FacultyId,
            ManagerId = managerId,
            ReviewedByAdminId = null,
            EventId = request.EventId,
            EventDetails = request.EventDetails,
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = request.RequestType,
            Status = RequestStatus.Pending
        };
        await _eventRequestRepository.AddAsync(eventRequest);
        return eventRequest.Id;
    }

    public async Task DeleteAsync(Guid managerId, Guid requestId)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);
        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.ManagerId != managerId)
            throw new UnauthorizedAccessException("You do not have permission to delete this event request.");

        if (eventRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending event requests can be deleted.");
        await _eventRequestRepository.DeleteAsync(requestId);
    }

    public async Task<List<EventRequestResponse>> GetByManagerIdAsync(Guid managerId)
    {
        return (await _eventRequestRepository.GetByManagerIdAsync(managerId))
            .Select(e => e.ToEventRequestResponse())
            .ToList();
    }

    public async Task<EventRequestResponse?> GetByIdAsync(Guid userId, Guid requestId)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.ManagerId != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this request.");

        return eventRequest.ToEventRequestResponse();
    }

    public async Task<List<EventRequestResponse>> GetAllAsync()
    {
        return (await _eventRequestRepository.GetAllAsync())
            .Select(e => e.ToEventRequestResponse())
            .ToList();
    }

    public async Task ApproveAsync(Guid adminId, Guid requestId)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        eventRequest.Status = RequestStatus.Approved;
        eventRequest.ReviewedByAdminId = adminId;
        eventRequest.ReviewedAt = DateTime.UtcNow;

        if (!_strategies.TryGetValue(eventRequest.RequestType, out var strategy))
        {
            throw new NotSupportedException($"Strategy for request type {eventRequest.RequestType} is not supported.");
        }

        await strategy.ExecuteAsync(eventRequest);

        await _eventRequestRepository.UpdateAsync(eventRequest);
    }

    public async Task RejectAsync(Guid adminId, Guid requestId)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        eventRequest.Status = RequestStatus.Rejected;
        eventRequest.ReviewedByAdminId = adminId;
        eventRequest.ReviewedAt = DateTime.UtcNow;

        await _eventRequestRepository.UpdateAsync(eventRequest);
    }
}
