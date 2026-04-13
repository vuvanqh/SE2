using System.ComponentModel.DataAnnotations;
using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests;

public record CreateEventRequestRequest
{
    [Required] public Guid FacultyId { get; init; }
    public Guid? EventId { get; init; }
    [Required] public RequestType RequestType { get; init; }
    [Required] public EventDetailsDto EventDetails { get; init; } = null!;
}
