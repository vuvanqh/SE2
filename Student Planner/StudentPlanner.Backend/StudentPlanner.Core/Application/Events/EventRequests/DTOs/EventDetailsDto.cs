using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.EventRequests;

public record EventDetailsDto
{
    [Required] public string Title { get; init; } = null!;
    [Required] public DateTime StartTime { get; init; }
    [Required] public DateTime EndTime { get; init; }
    public string? Location { get; init; }
    public string? Description { get; init; }
}
