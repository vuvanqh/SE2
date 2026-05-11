using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IAcademicEventRepository
{
    Task<AcademicEvent?> GetByIdAsync(Guid eventId);
    Task<IEnumerable<AcademicEvent>> GetAllAsync();
    Task<IEnumerable<AcademicEvent>> GetAllPagedAsync(int skip, int take);
    Task<int> CountAllAsync();
    Task<IEnumerable<AcademicEvent>> GetByFacultyIdAsync(Guid facultyId);
    Task<IEnumerable<AcademicEvent>> GetByFacultyIdPagedAsync(Guid facultyId, int skip, int take);
    Task<int> CountByFacultyIdAsync(Guid facultyId);
    Task<IEnumerable<AcademicEvent>> GetUniversityEventsAsync();
    Task<IEnumerable<AcademicEvent>> GetUniversityEventsPagedAsync(int skip, int take);
    Task<int> CountUniversityEventsAsync();
    Task<IEnumerable<AcademicEvent>> GetByFacultiesAsync(List<Guid> facultyIds);
    Task<IEnumerable<AcademicEvent>> GetByFacultiesPagedAsync(List<Guid> facultyIds, int skip, int take);
    Task<int> CountByFacultiesAsync(List<Guid> facultyIds);
    Task AddAsync(AcademicEvent academicEvent);
    Task UpdateAsync(AcademicEvent academicEvent);
    Task DeleteAsync(Guid eventId);
    Task<HashSet<Guid>> GetSubscribedEventIdsAsync(Guid userId);
    Task<bool> IsSubscribedAsync(Guid eventId, Guid userId);
    Task SubscribeAsync(Guid eventId, Guid userId);
    Task UnsubscribeAsync(Guid eventId, Guid userId);
}
