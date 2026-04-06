using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.EventRequests;

public class EventRequestService : IEventRequestService
{
    private readonly IEventRequestRepository _eventRequestRepository;

    public EventRequestService(IEventRequestRepository eventRequestRepository)
    {
        _eventRequestRepository = eventRequestRepository;
    }

    public async Task<Guid> CreateAsync(Guid managerId, CreateEventRequestRequest request)
    {
        EventRequest eventRequest = new EventRequest
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            FacultyId = request.FacultyId,
            ManagerId = managerId,
            RequestType = request.RequestType,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ReviewedByAdminId = null,
            ReviewedAt = null,
            ReviewComment = null,
            //Event = null!
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

    public async Task ApproveAsync(Guid adminId, Guid requestId, ReviewEventRequestRequest request)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        eventRequest.Status = RequestStatus.Approved;
        eventRequest.ReviewedByAdminId = adminId;
        eventRequest.ReviewedAt = DateTime.UtcNow;
        eventRequest.ReviewComment = request.ReviewComment;

        // Here create or update the actual event based on the request details

        await _eventRequestRepository.UpdateAsync(eventRequest);
    }

    public async Task RejectAsync(Guid adminId, Guid requestId, ReviewEventRequestRequest request)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (eventRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        eventRequest.Status = RequestStatus.Rejected;
        eventRequest.ReviewedByAdminId = adminId;
        eventRequest.ReviewedAt = DateTime.UtcNow;
        eventRequest.ReviewComment = request.ReviewComment;

        await _eventRequestRepository.UpdateAsync(eventRequest);
    }
}