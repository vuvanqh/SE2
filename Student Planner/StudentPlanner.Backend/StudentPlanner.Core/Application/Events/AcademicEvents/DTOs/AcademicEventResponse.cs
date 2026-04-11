using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.AcademicEvents.DTOs;

public record AcademicEventResponse
{
    [Required] public Guid Id { get; set; }
    [Required] public Guid FacultyId { get; set; }
    [Required] public string Title { get; set; } = null!;
    [Required] public string? Description { get; set; }
    [Required] public DateTime StartTime { get; set; }
    [Required] public DateTime EndTime { get; set; }
    [Required] public string? Location { get; set; }
}
