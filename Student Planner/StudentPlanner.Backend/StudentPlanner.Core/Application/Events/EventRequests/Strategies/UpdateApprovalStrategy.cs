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
        var existingEvent = await _academicEventRepository.GetByIdAsync(eventRequest.EventId.Value);
        if (existingEvent == null) throw new ArgumentException("The target event for this update does not exist.");

        EventRequestValidationHelper.ValidateEventDetails(eventRequest.EventDetails);

        if (existingEvent is FacultyEvent fe)
        {
            if (!eventRequest.FacultyId.HasValue || eventRequest.FacultyId.Value != fe.FacultyId)
            {
                throw new InvalidOperationException("Cannot change the faculty of an existing Faculty Event.");
            }
        }
        else if (existingEvent is UniversityEvent && eventRequest.FacultyId.HasValue)
        {
            throw new InvalidOperationException("Cannot change a University Event to a Faculty Event.");
        }

        existingEvent.EventDetails = eventRequest.EventDetails;
        await _academicEventRepository.UpdateAsync(existingEvent);
    }
}
