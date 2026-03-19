public class ResetPasswordDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Email is not in a valid email format")]
    public string Email { get; set; } = null!;
    [Required]
    public string Token { get; set; } = null!; 
    [Required]
    public string NewPassword { get; set; } = null!;
    [Required]
    [Compare("Password", ErrorMessage ="Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = null!;
}