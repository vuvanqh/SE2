using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;

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

        var events = user.Role == UserRoleOptions.Admin ?
            (await _academicEventRepo.GetAllAsync()) :
            (await _academicEventRepo.GetByFacultyIdAsync(user.FacultyId!.Value));

        return events.Select(e => new EventPreveiwDto
        {
            EndTime = e.EventDetails.EndTime,
            StartTime = e.EventDetails.StartTime,
            Id = e.Id,
            Location = e.EventDetails.Location,
            Title = e.EventDetails.Title,
            EventType = ValueObjects.EventPreveiwType.AcademicEvent
        });

    }
}
