using System;
using System.Threading.Tasks;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public class CreateApprovalStrategy : IEventRequestApprovalStrategy
{
    private readonly IAcademicEventRepository _academicEventRepository;

    public CreateApprovalStrategy(IAcademicEventRepository academicEventRepository)
    {
        _academicEventRepository = academicEventRepository;
    }

    public RequestType RequestType => RequestType.Create;

    public async Task ExecuteAsync(EventRequest eventRequest)
    {
        EventRequestValidationHelper.ValidateEventDetails(eventRequest.EventDetails);

        AcademicEvent newEvent;
        if (eventRequest.FacultyId.HasValue)
        {
            newEvent = new FacultyEvent
            {
                Id = Guid.NewGuid(),
                FacultyId = eventRequest.FacultyId.Value,
                EventDetails = eventRequest.EventDetails
            };
        }
        else
        {
            newEvent = new UniversityEvent
            {
                Id = Guid.NewGuid(),
                EventDetails = eventRequest.EventDetails
            };
        }

        await _academicEventRepository.AddAsync(newEvent);
        eventRequest.EventId = newEvent.Id;
    }
}
