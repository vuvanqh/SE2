namespace StudentPlanner.Core.Domain;

public class EventRequest
{
    public required Guid Id { get; set; }
    public required Guid FacultyId { get; set; }
    public required Guid ManagerId { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public required Guid EventId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public required RequestType RequestType { get; set; }
    public required RequestStatus Status { get; set; }
    public string? ReviewComment { get; set; }
    //public required AcademicEvent Event { get; set; }
}

