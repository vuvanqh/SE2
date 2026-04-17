using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public class PersonalEventPreveiwStrategy : IEventPreviewStrategy
{
    private readonly IPersonalEventRepository _personalEventRepo;
    public PersonalEventPreveiwStrategy(IPersonalEventRepository personalEventRepo)
    {
        _personalEventRepo = personalEventRepo;
    }

    public bool CanHandle(UserContext user) => user.Role == UserRoleOptions.Student;

    public async Task<IEnumerable<EventPreveiwDto>> GetAsync(UserContext user, EventPreviewQuery query)
    {
        var events = await _personalEventRepo.GetEventsByUserIdAsync(user.Id);
        return events.Select(e => new EventPreveiwDto()
        {
            EndTime = e.EventDetails.EndTime,
            StartTime = e.EventDetails.StartTime,
            Id = e.Id,
            Location = e.EventDetails.Location,
            Title = e.EventDetails.Title,
            EventType = ValueObjects.EventPreveiwType.PersonalEvent
        });
    }
}
