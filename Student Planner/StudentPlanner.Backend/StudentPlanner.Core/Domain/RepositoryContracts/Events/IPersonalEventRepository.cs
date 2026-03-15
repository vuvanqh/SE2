using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IPersonalEventRepository
{
    Task<List<PersonalEvent>> GetEventsByUserIdAsync(Guid userId);
    Task<PersonalEvent?> GetEventByEventIdAsync(Guid eventId);
    Task AddAsync(PersonalEvent personalEvent);
    Task DeleteAsync(Guid eventId);
    Task UpdateAsync(PersonalEvent personalEvent);
}
