using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.Authentication;

public record LoginResponseDto
{
    //session
    public string Token { get; set; } = null!;
    public string UserRole { get; set; } = null!;

    //data
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    [EmailAddress]
    public string? Email { get; set; } = null!;
    public string? UsosToken {get; set;} = null!;
}