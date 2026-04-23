using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IAcademicEventRepository
{
    Task<AcademicEvent?> GetByIdAsync(Guid eventId);
    Task<IEnumerable<AcademicEvent>> GetAllAsync();
    Task<IEnumerable<AcademicEvent>> GetByFacultyIdAsync(Guid facultyId);
    Task AddAsync(AcademicEvent academicEvent);
    Task UpdateAsync(AcademicEvent academicEvent);
    Task DeleteAsync(Guid eventId);
    Task<bool> IsSubscribedAsync(Guid eventId, Guid userId);
    Task SubscribeAsync(Guid eventId, Guid userId);
    Task<bool> UnsubscribeAsync(Guid eventId, Guid userId);
}
