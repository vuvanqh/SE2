using System.ComponentModel.DataAnnotations;
namespace StudentPlanner.Core.Application.Authentication;

public record ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}