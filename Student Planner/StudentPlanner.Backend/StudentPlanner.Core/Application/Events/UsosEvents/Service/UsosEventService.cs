using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;
using  StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.Events.UsosEvents.Mapping;
namespace StudentPlanner.Core.Application.Events.UsosEvents.Services;

public class UsosEventService : IUsosEventService
{
    private readonly IUsosClient _usosClient;
    private readonly IUsosEventRepository _studentUsosEventRepository;

    public UsosEventService(
        IUsosClient usosClient,
        IUsosEventRepository studentUsosEventRepository)
    {
        _usosClient = usosClient;
        _studentUsosEventRepository = studentUsosEventRepository;
    }

    public async Task<List<UsosEventResponseDto>> SyncAndGetEventsAsync(Guid userId, string usosToken, DateOnly start, int days)
    {
        var fetchedEvents = await _usosClient.GetTimetableAsync(usosToken, start, days);

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