using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;
using StudentPlanner.Core.Domain.RepositoryContracts;
using Microsoft.Extensions.Caching.Memory;
namespace StudentPlanner.Core.Application.Events.UsosEvents.Services;

public class UsosEventService : IUsosEventService
{
    private readonly IUsosClient _usosClient;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    public UsosEventService(
        IUsosClient usosClient,
        IMemoryCache cache,
        IUserRepository userRepository
        )
    {
        _usosClient = usosClient;
        _cache = cache;
        _userRepository = userRepository;
    }

    public async Task<List<UsosEventResponseDto>> SyncAndGetEventsAsync(Guid userId, DateOnly start, int days)
    {
        string cacheKey = $"usos-events-{userId}-{start:yyyyMMdd}-{days}";
        if (_cache.TryGetValue(cacheKey, out List<UsosEventResponseDto>? cachedEvents))
        {
            return cachedEvents!;
        }
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.UsosToken))
            throw new InvalidOperationException("User does not have a linked USOS token.");

        var fetchedEvents = await _usosClient.GetTimetableAsync(user.UsosToken, start, days);

        foreach (var e in fetchedEvents)
        {
            string cacheKey2 = $"{userId}-{e.Id}";
            _cache.Set(
                cacheKey2,
                e,
                TimeSpan.FromMinutes(30)
            );
        }
        _cache.Set(
            cacheKey,
            fetchedEvents,
            TimeSpan.FromMinutes(30) // the duration of the token should be matched with the duration of the jwt token
        );
        return fetchedEvents;
    }

    public async Task<UsosEventResponseDto> GetEventByIdAsync(Guid userId, string eventId)
    {
        var cacheKey = $"event:{userId}:{eventId}";

        if (_cache.TryGetValue(cacheKey, out UsosEventResponseDto? cachedEvent))
            return cachedEvent!;


        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.UsosToken))
            throw new InvalidOperationException("User does not have a linked USOS token.");

        var fetchedEvent = await _usosClient.GetEventAsync(user.UsosToken, eventId);

        _cache.Set(
            cacheKey,
            fetchedEvent,
            TimeSpan.FromMinutes(30));

        return fetchedEvent;
    }
}