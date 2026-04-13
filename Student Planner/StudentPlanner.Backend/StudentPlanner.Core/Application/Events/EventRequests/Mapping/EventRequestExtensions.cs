using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests;

public static class EventRequestExtensions
{
    public static EventDetails ToEventDetails(this EventDetailsDto dto)
    {
        return new EventDetails
        {
            Title = dto.Title,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Location = dto.Location,
            Description = dto.Description
        };
    }

    public static EventDetailsDto ToEventDetailsDto(this EventDetails details)
    {
        return new EventDetailsDto
        {
            Title = details.Title,
            StartTime = details.StartTime,
            EndTime = details.EndTime,
            Location = details.Location,
            Description = details.Description
        };
    }

    public static EventRequest ToEventRequest(this CreateEventRequestRequest request, Guid managerId)
    {
        return new EventRequest
        {
            Id = Guid.NewGuid(),
            FacultyId = request.FacultyId,
            ManagerId = managerId,
            ReviewedByAdminId = null,
            EventId = request.EventId,
            EventDetails = request.EventDetails.ToEventDetails(),
            CreatedAt = DateTime.UtcNow,
            ReviewedAt = null,
            RequestType = request.RequestType,
            Status = RequestStatus.Pending
        };
    }

    public static EventRequestResponse ToEventRequestResponse(this EventRequest eventRequest)
    {
        return new EventRequestResponse
        {
            Id = eventRequest.Id,
            FacultyId = eventRequest.FacultyId,
            ManagerId = eventRequest.ManagerId,
            ReviewedByAdminId = eventRequest.ReviewedByAdminId,
            EventId = eventRequest.EventId,
            EventDetails = eventRequest.EventDetails.ToEventDetailsDto(),
            RequestType = eventRequest.RequestType,
            Status = eventRequest.Status,
            CreatedAt = eventRequest.CreatedAt,
            ReviewedAt = eventRequest.ReviewedAt
        };
    }
}