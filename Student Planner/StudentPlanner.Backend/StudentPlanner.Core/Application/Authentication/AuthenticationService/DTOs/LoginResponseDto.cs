using System;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.Authentication;

public record LoginResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    //session
    public string? AccessToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    //data
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress] 
    public string? Email { get; set; }
}