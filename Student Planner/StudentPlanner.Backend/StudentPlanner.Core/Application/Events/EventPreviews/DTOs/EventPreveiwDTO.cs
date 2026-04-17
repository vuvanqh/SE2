using StudentPlanner.Core.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.Events.EventPreveiws;

public record EventPreveiwDto
{
    [Required] public Guid Id { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string? Location { get; set; }
    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
    [Required] public EventPreveiwType EventType { get; set; }
}
