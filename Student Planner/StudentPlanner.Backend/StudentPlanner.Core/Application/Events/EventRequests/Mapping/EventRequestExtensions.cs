using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests;

public static class EventRequestExtensions
{
    public static EventRequestResponse ToEventRequestResponse(this EventRequest eventRequest)
    {
        return new EventRequestResponse
        {
            Id = eventRequest.Id,
            FacultyId = eventRequest.FacultyId,
            ManagerId = eventRequest.ManagerId,
            ReviewedByAdminId = eventRequest.ReviewedByAdminId,
            RequestType = eventRequest.RequestType,
            Status = eventRequest.Status,
            CreatedAt = eventRequest.CreatedAt,
            ReviewedAt = eventRequest.ReviewedAt,
            ReviewComment = eventRequest.ReviewComment,
            EventId = eventRequest.EventId
        };
    }
}
