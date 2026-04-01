using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Infrastructure.Clients.DTO;

public record FacultyResponse
{
    [Required] public string faculty_id { get; init; } = null!;
    [Required] public string faculty_name { get; init; } = null!;
    [Required] public string faculty_code { get; init; } = null!;
}
