using StudentPlanner.Core.Entities;
namespace StudentPlanner.Core.Application.Events;

public record UserContext
{
    public required Guid Id { get; init; }
    public required UserRoleOptions Role { get; init; }
    public Guid? FacultyId { get; set; }
}
