using StudentPlanner.Core;

namespace StudentPlanner.Core.Domain;

public class UsosEvent : Event
{
    public Guid UserId { get; set; }

    public string? CourseId { get; set; }
    public string? ClassType { get; set; }
    public string? GroupNumber { get; set; }

    public string? BuildingId { get; set; }
    public string? BuildingName { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomId { get; set; }

    public required string ExternalKey { get; set; }
}