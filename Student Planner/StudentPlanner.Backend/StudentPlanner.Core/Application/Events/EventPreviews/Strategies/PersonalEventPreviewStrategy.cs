using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System.Linq;
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
        
        var from = query.From ?? DateTime.UtcNow.Date;
        var to = from.AddDays(query.Days ?? 31);

        var filteredEvents = events.Where(e => e.EventDetails.StartTime >= from && e.EventDetails.StartTime <= to).ToList();

        Console.WriteLine($"[PersonalEventPreviewStrategy] User {user.Id}: Found {events.Count} total events, {filteredEvents.Count} after date filtering ({from} to {to})");

        return filteredEvents.Select(e => new EventPreveiwDto()
        {
            EndTime = e.EventDetails.EndTime,
            StartTime = e.EventDetails.StartTime,
            Id = e.Id.ToString(),
            Location = e.EventDetails.Location,
            Title = e.EventDetails.Title,
            EventType = ValueObjects.EventPreveiwType.PersonalEvent
        });
    }
}
