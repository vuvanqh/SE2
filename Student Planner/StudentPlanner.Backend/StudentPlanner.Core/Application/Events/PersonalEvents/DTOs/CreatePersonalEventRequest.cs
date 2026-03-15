using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Application.PersonalEvents;

public record CreatePersonalEventRequest
{
    [Required] public string Title { get; set; } = null!;
    [Required] public string? Description { get; set; }
    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
    [Required] public string? Location { get; set; }
}
