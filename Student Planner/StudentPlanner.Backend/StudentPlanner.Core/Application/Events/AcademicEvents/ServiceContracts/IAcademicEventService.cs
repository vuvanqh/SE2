using StudentPlanner.Core.Application.AcademicEvents.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentPlanner.Core.Application.AcademicEvents.ServiceContracts;

public interface IAcademicEventService
{
    Task<IEnumerable<AcademicEventResponse>> GetAccessibleEventsAsync(Guid id, string role, List<Guid>? facultyIds);
    Task<AcademicEventResponse?> GetEventByIdAsync(Guid id, Guid userId);
    Task SubscribeAsync(Guid eventId, Guid userId);
    Task UnsubscribeAsync(Guid eventId, Guid userId);
}
