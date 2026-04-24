using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.Events.UsosEvents.Mapping;
namespace StudentPlanner.Core.Application.Events.UsosEvents.Services;

public class UsosEventService : IUsosEventService
{
    private readonly IUsosClient _usosClient;
    private readonly IUsosEventRepository _studentUsosEventRepository;
    private readonly IUserRepository _userRepository;

    public UsosEventService(
        IUsosClient usosClient,
        IUsosEventRepository studentUsosEventRepository,
        IUserRepository userRepository)
    {
        _usosClient = usosClient;
        _studentUsosEventRepository = studentUsosEventRepository;
        _userRepository = userRepository;
    }

    public async Task<List<UsosEventResponseDto>> SyncAndGetEventsAsync(Guid userId, DateOnly start, int days)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.UsosToken))
            throw new InvalidOperationException("User does not have a linked USOS token.");

        var fetchedEvents = await _usosClient.GetTimetableAsync(user.UsosToken, start, days);

        var from = start.ToDateTime(TimeOnly.MinValue);
        var to = start.AddDays(days).ToDateTime(TimeOnly.MaxValue);

        await _studentUsosEventRepository.DeleteByUserAndRangeAsync(userId, from, to);

        var entities = fetchedEvents
            .Select(x => x.ToStudentUsosEvent(userId))
            .ToList();

        await _studentUsosEventRepository.AddRangeAsync(entities);
        await _studentUsosEventRepository.SaveChangesAsync();

        return fetchedEvents;
    }
}