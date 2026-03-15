using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

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

    public Task<PersonalEventResponse?> GetEventByIdAsync(Guid userId, Guid eventId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PersonalEventResponse>> GetEventsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdatePersonalEventAsync(Guid userId, Guid eventId, UpdatePersonalEventRequest request)
    {
        PersonalEvent? personalEvent = await _personalEventRepo.GetEventByEventIdAsync(eventId);
        PersonalEventPolicy.EnsureHasPermissions(userId, personalEvent);

        personalEvent = request.ToPersonalEvent(userId, eventId);

        PersonalEventPolicy.EnsureValidEvent(personalEvent);
        await _personalEventRepo.UpdateAsync(personalEvent);
    }
}
