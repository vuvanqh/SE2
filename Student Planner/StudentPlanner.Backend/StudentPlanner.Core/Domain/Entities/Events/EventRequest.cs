using StudentPlanner.Core;

namespace StudentPlanner.Core.Domain;

public class EventRequest
{
    public required Guid Id { get; set; }
    public Guid? FacultyId { get; set; }
    public required Guid ManagerId { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public Guid? EventId { get; set; }
    public required EventDetails EventDetails { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public required RequestType RequestType { get; set; }
    public required RequestStatus Status { get; set; }
}
