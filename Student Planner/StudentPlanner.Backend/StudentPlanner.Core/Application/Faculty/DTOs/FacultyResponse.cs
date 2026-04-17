using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Application.DTOs;

public class FacultyResponse
{
    [Required] public Guid FacultyId { get; init; }
    [Required] public string FacultyName { get; init; } = null!;
    [Required] public string FacultyCode { get; init; } = null!;
}
