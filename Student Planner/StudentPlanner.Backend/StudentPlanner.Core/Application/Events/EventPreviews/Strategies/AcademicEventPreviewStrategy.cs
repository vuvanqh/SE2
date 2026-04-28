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
        IEnumerable<AcademicEvent> events;

        if (user.Role == UserRoleOptions.Admin)
        {
            if (query.FacultyIds == null || !query.FacultyIds.Any())
            {
                events = await _academicEventRepo.GetAllAsync();
            }
            else
            {
                var hasUniversity = query.FacultyIds.Contains(Guid.Empty);
                var actualFacultyIds = query.FacultyIds.Where(id => id != Guid.Empty).ToList();

                var facultyEvents = actualFacultyIds.Any()
                    ? await _academicEventRepo.GetByFacultiesAsync(actualFacultyIds)
                    : Enumerable.Empty<AcademicEvent>();

                var universityEvents = hasUniversity
                    ? await _academicEventRepo.GetUniversityEventsAsync()
                    : Enumerable.Empty<AcademicEvent>();

                events = facultyEvents.Concat(universityEvents);
            }
        }
        else if (user.Role == UserRoleOptions.Manager)
        {
            if (user.FacultyId == null)
            {
                // University Manager
                events = await _academicEventRepo.GetUniversityEventsAsync();
            }
            else
            {
                // Faculty Manager
                events = await _academicEventRepo.GetByFacultyIdAsync(user.FacultyId.Value);
            }
        }
        else if (user.Role == UserRoleOptions.Student)
        {
            // Students see their faculty events + university events
            var universityEvents = await _academicEventRepo.GetUniversityEventsAsync();
            var facultyEvents = user.FacultyId.HasValue 
                ? await _academicEventRepo.GetByFacultyIdAsync(user.FacultyId.Value)
                : Enumerable.Empty<AcademicEvent>();
            
            events = universityEvents.Concat(facultyEvents);
        }
        else
        {
            events = Enumerable.Empty<AcademicEvent>();
        }

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
