using StudentPlanner.Core.Domain;
using StudentPlanner.Core;

namespace StudentPlanner.Core.Application.PersonalEvents;

public static class CreatePersonalEventRequestExtention
{
    public static PersonalEvent ToPersonalEvent(this CreatePersonalEventRequest request, Guid userId)
    {
        return new PersonalEvent()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventDetails = new EventDetails()
            {
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location!,
                Description = request.Description
            }
        };
    }
}

public static class UpdatePersonalEventRequestExtention
{
    public static PersonalEvent ToPersonalEvent(this UpdatePersonalEventRequest request, Guid userId, Guid eventId)
    {
        return new PersonalEvent()
        {
            Id = eventId,
            UserId = userId,
            EventDetails = new EventDetails()
            {
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location!,
                Description = request.Description
            }
        };
    }
}

public static class PersonalEventExtention
{
    public static PersonalEventResponse ToPersonalEventResponse(this PersonalEvent personalEvent)
    {
        EventDetails eventDetails = personalEvent.EventDetails;
        return new PersonalEventResponse()
        {
            Id = personalEvent.Id,
            Title = eventDetails.Title,
            StartTime = eventDetails.StartTime,
            EndTime = eventDetails.EndTime,
            Location = eventDetails.Location,
            Description = eventDetails.Description
        };
    }
}

