using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.PersonalEvents;

public interface IPersonalEventService
{
    Task<PersonalEventResponse?> GetEventByIdAsync(Guid userId, Guid eventId);
    Task<List<PersonalEventResponse>> GetEventsAsync(Guid userId);
    Task<Guid> CreatePersonalEventAsync(Guid userId, CreatePersonalEventRequest request);
    Task UpdatePersonalEventAsync(Guid userId, Guid eventId, UpdatePersonalEventRequest request);
    Task DeletePersonalEventAsync(Guid userId, Guid eventId);
}
