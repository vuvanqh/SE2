using System;
using System.Threading.Tasks;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public class UpdateApprovalStrategy : IEventRequestApprovalStrategy
{
    private readonly IAcademicEventRepository _academicEventRepository;

    public UpdateApprovalStrategy(IAcademicEventRepository academicEventRepository)
    {
        _academicEventRepository = academicEventRepository;
    }

    public RequestType RequestType => RequestType.Update;

    public async Task ExecuteAsync(EventRequest eventRequest)
    {
        if (eventRequest.EventId == null) throw new InvalidOperationException("Update request missing EventId.");
        var existingUpdateEvent = await _academicEventRepository.GetByIdAsync(eventRequest.EventId.Value);
        if (existingUpdateEvent == null) throw new ArgumentException("The target event for this update does not exist.");

        EventRequestValidationHelper.ValidateEventDetails(eventRequest.EventDetails);
        existingUpdateEvent.EventDetails = eventRequest.EventDetails;
        existingUpdateEvent.FacultyId = eventRequest.FacultyId;
        await _academicEventRepository.UpdateAsync(existingUpdateEvent);
    }
}
