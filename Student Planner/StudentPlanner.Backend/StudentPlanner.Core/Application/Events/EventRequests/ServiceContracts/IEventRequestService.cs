using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests;

public interface IEventRequestService
{
    Task<Guid> CreateAsync(Guid managerId, CreateEventRequestRequest request);
    Task DeleteAsync(Guid managerId, Guid requestId);
    Task<List<EventRequestResponse>> GetByManagerIdAsync(Guid managerId);
    Task<EventRequestResponse?> GetByIdAsync(Guid userId, Guid requestId);
    Task<List<EventRequestResponse>> GetAllAsync();
    Task ApproveAsync(Guid adminId, Guid requestId, ReviewEventRequestRequest request);
    Task RejectAsync(Guid adminId, Guid requestId, ReviewEventRequestRequest request);
}