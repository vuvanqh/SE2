namespace StudentPlanner.Core.Application.Admin.DTO;

public class UsersResultDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserRole { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FacultyCode { get; set; }
}