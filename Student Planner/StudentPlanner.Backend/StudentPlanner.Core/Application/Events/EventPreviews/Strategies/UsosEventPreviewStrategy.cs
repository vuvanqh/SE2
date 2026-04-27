using Microsoft.Extensions.Caching.Memory;
using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System.Globalization;

namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public class UsosEventPreviewStrategy : IEventPreviewStrategy
{
    private readonly IMemoryCache _cache;
    private readonly IUsosClient _usosClient;
    private readonly IUserRepository _userRepository;
    public UsosEventPreviewStrategy(IMemoryCache cache, IUsosClient usosClient, IUserRepository userRepository)
    {
        _cache = cache;
        _userRepository = userRepository;
        _usosClient = usosClient;
    }
    public bool CanHandle(UserContext user) => user.Role == UserRoleOptions.Student;
    public async Task<IEnumerable<EventPreveiwDto>> GetAsync(UserContext user, EventPreviewQuery query)
    {
        var now = DateTime.UtcNow;
        var from = query.From ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var days = query.Days ?? 31;
        string cacheKey = $"usos-preview:{user.Id}:{from:yyyyMMdd}:{days}";

        IEnumerable<UsosEventResponseDto> events;

        if (_cache.TryGetValue(cacheKey, out List<UsosEventResponseDto>? cached))
        {
            events = cached!;
        }
        else
        {
            var userEntity = await _userRepository.GetByIdAsync(user.Id);

            if (userEntity == null)
                throw new KeyNotFoundException("User doesn't exist");
            if (string.IsNullOrWhiteSpace(userEntity.UsosToken))
                throw new InvalidOperationException("User does not have a linked USOS token.");

            events = await _usosClient.GetTimetableAsync(userEntity.UsosToken, DateOnly.FromDateTime(from), days);
        }

        var previews = events.Select(e => new EventPreveiwDto
        {
            Id = e.Id,
            Title = e.Title!,
            Location = $"{e.BuildingName} - {e.RoomNumber}",
            EndTime = DateTime.ParseExact(e.EndTime!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            StartTime = DateTime.ParseExact(e.StartTime!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            EventType = ValueObjects.EventPreveiwType.UsosEvent
        }).ToList();

        _cache.Set(cacheKey, previews, TimeSpan.FromMinutes(30));

        return previews;
    }
}