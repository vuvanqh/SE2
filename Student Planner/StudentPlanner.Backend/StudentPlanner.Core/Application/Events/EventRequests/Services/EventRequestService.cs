using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.EventRequests.Strategies;
using System.Collections.Generic;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.Core.Application.EventRequests;

public class EventRequestService : IEventRequestService
{
    private readonly IEventRequestRepository _eventRequestRepository;
    private readonly IReadOnlyDictionary<RequestType, IEventRequestApprovalStrategy> _strategies;
    private readonly IEventRequestNotificationService _notify;
    private readonly IUserRepository _userRepository;

    public EventRequestService(
        IEventRequestRepository eventRequestRepository,
        IEnumerable<IEventRequestApprovalStrategy> strategies,
        IEventRequestNotificationService notify,
        IUserRepository userRepository)
    {
        _eventRequestRepository = eventRequestRepository;
        _strategies = strategies.ToDictionary(s => s.RequestType, s => s);
        _notify = notify;
        _userRepository = userRepository;
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
        var manager = await _userRepository.GetByIdAsync(managerId);
        if (manager == null) throw new KeyNotFoundException("Manager not found.");

        if (manager.Faculty?.Id != request.FacultyId)
        {
            throw new UnauthorizedAccessException("You do not have permission to create event requests for this scope.");
        }

        EventRequest eventRequest = request.ToEventRequest(managerId);
        await _eventRequestRepository.AddAsync(eventRequest);

        await _notify.NotifyEventRequestListChanged();
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
        await _notify.NotifyEventRequestListChanged();
    }

    public async Task<List<EventRequestResponse>> GetByManagerIdAsync(Guid managerId)
    {
        return (await _eventRequestRepository.GetByManagerIdAsync(managerId))
            .Select(e => e.ToEventRequestResponse())
            .ToList();
    }

    public async Task<EventRequestResponse?> GetByIdAsync(Guid requestId, Guid userId, string role)
    {
        EventRequest? eventRequest = await _eventRequestRepository.GetByIdAsync(requestId);

        if (eventRequest == null)
            throw new ArgumentException("Event request does not exist.");

        if (role != UserRoleOptions.Admin.ToString() && eventRequest.ManagerId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this event request.");
        }

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

        var users = await _userRepository.GetFacultyUsersAsync(eventRequest.FacultyId);
        foreach(var u in users)
        {
            string message;
            switch (eventRequest.RequestType)
            {
                case RequestType.Create: message = "A new academic event has been added"; break;
                case RequestType.Update: message = $"An academic event {eventRequest.EventDetails.Title!} has been updated"; break;
                default: message = "Academic event notification"; break;
            }
            await _notify.AcademicEventNotification(u.Id, message, eventRequest.FacultyId);
        }
        await _notify.EventRequestUpdated(eventRequest.ManagerId);
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
        await _notify.EventRequestUpdated(eventRequest.ManagerId);
    }
}
