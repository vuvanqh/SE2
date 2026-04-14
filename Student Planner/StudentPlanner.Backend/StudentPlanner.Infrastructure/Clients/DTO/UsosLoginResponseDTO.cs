using StudentPlanner.Core.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Infrastructure.Clients.DTO;

public record UsosLoginResponseDto
{
    [Required] public string token { get; set; } = null!;
    [Required] public string firstName { get; set; } = null!;
    [Required] public string lastName { get; set; } = null!;
    [Required] public string facultyId { get; set; } = null!;

    public UsosLoginResponse ToUsosLoginResponse() => new UsosLoginResponse
    {
        Token = token,
        FirstName = firstName,
        LastName = lastName,
        FacultyId = facultyId
    };
}
