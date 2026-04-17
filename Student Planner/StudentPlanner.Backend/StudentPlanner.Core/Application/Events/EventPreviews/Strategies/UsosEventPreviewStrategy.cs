using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public class UsosEventPreviewStrategy : IEventPreviewStrategy
{
    private readonly IUsosEventRepository _usosReop;
    public UsosEventPreviewStrategy(IUsosEventRepository usosReop)
    {
        _usosReop = usosReop;
    }
    public bool CanHandle(UserContext user) => user.Role == UserRoleOptions.Student;
    public async Task<IEnumerable<EventPreveiwDto>> GetAsync(UserContext user, EventPreviewQuery query)
    {
        var now = DateTime.UtcNow;
        var from = query.From ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = query.To ?? from
        .AddMonths(1)
        .AddDays(-1);

        var events = await _usosReop.GetByUserAndRangeAsync(user.Id, from, to);

        return events.Select(e => new EventPreveiwDto
        {
            Id = e.Id,
            Title = e.EventDetails.Title,
            Location = $"{e.EventDetails.Location}, {e.BuildingName} - {e.RoomNumber}",
            EndTime = now,
            StartTime = now,
            EventType = ValueObjects.EventPreveiwType.UsosEvent
        }).ToList();
    }
}
