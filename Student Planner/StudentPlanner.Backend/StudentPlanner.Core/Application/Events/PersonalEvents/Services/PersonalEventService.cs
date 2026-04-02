using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using System.Runtime.CompilerServices;

namespace StudentPlanner.Core.Application.PersonalEvents;

public class PersonalEventService : IPersonalEventService
{
    private readonly IPersonalEventRepository _personalEventRepo;
    public PersonalEventService(IPersonalEventRepository personalEventRepo)
    {
        _personalEventRepo = personalEventRepo;
    }
    public async Task<Guid> CreatePersonalEventAsync(Guid userId, CreatePersonalEventRequest request)
    {
        PersonalEvent personalEvent = request.ToPersonalEvent(userId);
        PersonalEventPolicy.EnsureValidEvent(personalEvent);
        await _personalEventRepo.AddAsync(personalEvent);
        return personalEvent.Id;
    }

    public async Task DeletePersonalEventAsync(Guid userId, Guid eventId)
    {
        PersonalEvent? personalEvent = await _personalEventRepo.GetEventByEventIdAsync(eventId);
        PersonalEventPolicy.EnsureHasPermissions(userId, personalEvent);

        await _personalEventRepo.DeleteAsync(eventId);
    }

    public async Task<PersonalEventResponse?> GetEventByIdAsync(Guid userId, Guid eventId)
    {
        PersonalEvent? personalEvent = await _personalEventRepo.GetEventByEventIdAsync(eventId);
        PersonalEventPolicy.EnsureHasPermissions(userId, personalEvent);

        return personalEvent!.ToPersonalEventResponse();
    }

    public async Task<List<PersonalEventResponse>> GetEventsAsync(Guid userId)
    {
        return (await _personalEventRepo.GetEventsByUserIdAsync(userId))
            .Select(ev => ev.ToPersonalEventResponse()).ToList();
    }

    public async Task UpdatePersonalEventAsync(Guid userId, Guid eventId, UpdatePersonalEventRequest request)
    {
        PersonalEvent? personalEvent = await _personalEventRepo.GetEventByEventIdAsync(eventId);
        if (personalEvent == null)
            throw new ArgumentException("Invalid Event.");

        PersonalEventPolicy.EnsureHasPermissions(userId, personalEvent);

        EventDetails details = new EventDetails()
        {
            Title = request.Title,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Description = request.Description,
            Location = request.Location,
        };

        personalEvent.EventDetails = details;
        PersonalEventPolicy.EnsureValidEvent(personalEvent);
        await _personalEventRepo.UpdateAsync(personalEvent);
    }
}
