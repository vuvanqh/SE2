using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System.ComponentModel.Design;

namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public class AcademicEventPreviewStrategy : IEventPreviewStrategy
{
    private readonly IAcademicEventRepository _academicEventRepo;
    public AcademicEventPreviewStrategy(IAcademicEventRepository academicEventRepo)
    {
        _academicEventRepo = academicEventRepo;
    }
    public bool CanHandle(UserContext user) => true;

    public async Task<IEnumerable<EventPreveiwDto>> GetAsync(UserContext user, EventPreviewQuery query)
    {
        if (user.FacultyId == null && user.Role != UserRoleOptions.Admin)
            throw new InvalidDataException("FacultyId is required.");

        IEnumerable<AcademicEvent> events;
        if (user.Role == UserRoleOptions.Admin)
        {
            events = query.FacultyIds?.Count > 0 ?
                (await _academicEventRepo.GetByFacultiesAsync(query.FacultyIds)) :
                await _academicEventRepo.GetAllAsync();
        }
        else
            events = (await _academicEventRepo.GetByFacultyIdAsync(user.FacultyId!.Value));

        return events.Select(e => new EventPreveiwDto
        {
            EndTime = e.EventDetails.EndTime,
            StartTime = e.EventDetails.StartTime,
            Id = e.Id.ToString(),
            Location = e.EventDetails.Location,
            Title = e.EventDetails.Title,
            EventType = ValueObjects.EventPreveiwType.AcademicEvent
        });

    }
}
