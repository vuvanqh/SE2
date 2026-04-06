using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IEventRequestRepository
{
    Task<List<EventRequest>> GetAllAsync();
    Task<List<EventRequest>> GetByFacultyIdAsync(Guid facultyId);
    Task<List<EventRequest>> GetByManagerIdAsync(Guid managerId);
    Task<EventRequest?> GetByIdAsync(Guid requestId);
    Task AddAsync(EventRequest eventRequest);
    Task UpdateAsync(EventRequest eventRequest);
    Task DeleteAsync(Guid requestId);
}