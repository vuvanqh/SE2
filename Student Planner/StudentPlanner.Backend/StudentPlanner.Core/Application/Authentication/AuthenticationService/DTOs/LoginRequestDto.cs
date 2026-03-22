using System.ComponentModel.DataAnnotations;
namespace StudentPlanner.Core.Application.Authentication;

public record LoginRequestDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Email is not in a valid email format")]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = "Password cannot be blank")]
    public string Password { get; set; } = null!;
}