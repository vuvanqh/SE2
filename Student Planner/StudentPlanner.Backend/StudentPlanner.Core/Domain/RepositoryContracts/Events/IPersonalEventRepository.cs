using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IPersonalEventRepository
{
    Task<List<PersonalEvent>> GetPersonalEventsByUserIdAsync(Guid userId);
    Task<PersonalEvent?> GetPersonalEventByIdAsync(Guid eventId);
    Task DeletePersonalEventAsync(Guid eventId);
    Task UpdatePersonalEventAsync(PersonalEvent personalEvent);
}
