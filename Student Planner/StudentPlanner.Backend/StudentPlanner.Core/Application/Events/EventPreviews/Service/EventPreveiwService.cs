using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public class EventPreviewService : IEventPreviewService
{
    private readonly IEnumerable<IEventPreviewStrategy> _strategies;
    private readonly IUserRepository _userRepo;
    public EventPreviewService(IEnumerable<IEventPreviewStrategy> strategies, IUserRepository userRepo)
    {
        _strategies = strategies;
        _userRepo = userRepo;
    }

    public async Task<IEnumerable<EventPreveiwDto>> GetForUserAsync(UserContext user, EventPreviewQuery query)
    {
        var results = new List<EventPreveiwDto>();
        var u = await _userRepo.GetByIdAsync(user.Id);

        user.FacultyId = u?.Faculty?.Id;

        foreach (var strategy in _strategies.Where(s => s.CanHandle(user)))
        {
            var previews = await strategy.GetAsync(user, query);
            results.AddRange(previews);
        }

        return results;
    }
}
