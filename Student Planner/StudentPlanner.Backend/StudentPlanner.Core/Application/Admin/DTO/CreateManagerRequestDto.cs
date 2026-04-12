namespace StudentPlanner.Core.Application.Admin.DTO;

public class CreateManagerRequestDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FacultyId { get; set; } = null!; // Changable to just name
}
