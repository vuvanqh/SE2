using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Application.PersonalEvents;


//keep them separate in case of any future changes
public record UpdatePersonalEventRequest
{
    [Required] public Guid Id { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string? Description { get; set; }
    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
    [Required] public string? Location { get; set; }
}
