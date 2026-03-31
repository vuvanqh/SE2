using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.Authentication;

public record RegisterRequestDto
{
    /// <summary>
    /// User email address. Registration is restricted to @pw.edu.pl.
    /// </summary>
    [Required]
    [EmailAddress(ErrorMessage = "Email is not in a valid email format")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@pw\.edu\.pl$",
        ErrorMessage = "Registration is restricted to @pw.edu.pl email addresses.")]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = null!;
}