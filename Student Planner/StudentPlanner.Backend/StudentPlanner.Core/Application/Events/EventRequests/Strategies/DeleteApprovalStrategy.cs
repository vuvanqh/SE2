using System;
using System.Threading.Tasks;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public class DeleteApprovalStrategy : IEventRequestApprovalStrategy
{
    private readonly IAcademicEventRepository _academicEventRepository;

    public DeleteApprovalStrategy(IAcademicEventRepository academicEventRepository)
    {
        _academicEventRepository = academicEventRepository;
    }

    public RequestType RequestType => RequestType.Delete;

    public async Task ExecuteAsync(EventRequest eventRequest)
    {
        if (eventRequest.EventId == null) throw new InvalidOperationException("Delete request missing EventId.");
        var existingDeleteEvent = await _academicEventRepository.GetByIdAsync(eventRequest.EventId.Value);
        if (existingDeleteEvent == null) return;

        await _academicEventRepository.DeleteAsync(existingDeleteEvent.Id);
    }
}
