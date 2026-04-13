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
        var newEvent = new AcademicEvent
        {
            Id = Guid.NewGuid(),
            FacultyId = eventRequest.FacultyId,
            EventDetails = eventRequest.EventDetails
        };
        await _academicEventRepository.AddAsync(newEvent);
        eventRequest.EventId = newEvent.Id;
    }
}
