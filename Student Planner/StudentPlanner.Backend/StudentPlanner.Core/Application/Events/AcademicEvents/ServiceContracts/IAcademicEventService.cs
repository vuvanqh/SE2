using StudentPlanner.Core.Application.AcademicEvents.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentPlanner.Core.Application.AcademicEvents.ServiceContracts;

public interface IAcademicEventService
{
    Task<IEnumerable<AcademicEventResponse>> GetAllEventsAsync();
    Task<IEnumerable<AcademicEventResponse>> GetEventsForUserAsync(Guid userId);
    Task<AcademicEventResponse?> GetEventByIdAsync(Guid id, Guid userId);
}
