public class LoginResponseDto
{
    //session
    [Required] 
    public string AccessToken { get; set; } = null!;
    [Required] 
    public DateTime ExpiresAt { get; set; } = null!;

    //data
    [Required] 
    public string FirstName { get; set; } = null!;
    [Required] 
    public string Surname { get; set; } = null!;
    [Required,EmailAddress] 
    public string Email { get; set; } = null!;
}