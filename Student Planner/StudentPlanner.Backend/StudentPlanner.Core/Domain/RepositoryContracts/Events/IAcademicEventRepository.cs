using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IAcademicEventRepository
{
    Task<AcademicEvent?> GetByIdAsync(Guid eventId);
    Task<IEnumerable<AcademicEvent>> GetAllAsync();
    Task<IEnumerable<AcademicEvent>> GetByFacultyIdAsync(Guid facultyId);
    Task<IEnumerable<AcademicEvent>> GetByFacultiesAsync(List<Guid> facultyIds);
    Task AddAsync(AcademicEvent academicEvent);
    Task UpdateAsync(AcademicEvent academicEvent);
    Task DeleteAsync(Guid eventId);
    Task<HashSet<Guid>> GetSubscribedEventIdsAsync(Guid userId);
    Task<bool> IsSubscribedAsync(Guid eventId, Guid userId);
    Task SubscribeAsync(Guid eventId, Guid userId);
    Task UnsubscribeAsync(Guid eventId, Guid userId);
}
