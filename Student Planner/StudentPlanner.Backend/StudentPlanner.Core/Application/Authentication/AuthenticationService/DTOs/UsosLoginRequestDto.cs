namespace StudentPlanner.Core.Application.Authentication;

public record UsosLoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
