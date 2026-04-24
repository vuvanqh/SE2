using StudentPlanner.Core.Application.ClientContracts.DTO;

namespace StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;

public interface IUsosEventService
{
    Task<List<UsosEventResponseDto>> SyncAndGetEventsAsync(Guid userId, DateOnly start, int days);
}