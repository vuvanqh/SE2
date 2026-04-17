namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public interface IEventPreviewStrategy
{
    Task<IEnumerable<EventPreveiwDto>> GetAsync(UserContext user, EventPreviewQuery query);
    bool CanHandle(UserContext user);
}
