namespace StudentPlanner.Core.Application.Authentication;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}