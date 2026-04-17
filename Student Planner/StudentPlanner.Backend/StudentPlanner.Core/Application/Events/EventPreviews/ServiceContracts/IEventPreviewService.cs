namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public interface IEventPreviewService
{
    Task<IEnumerable<EventPreveiwDto>> GetForUserAsync(UserContext user, EventPreviewQuery query);
}
