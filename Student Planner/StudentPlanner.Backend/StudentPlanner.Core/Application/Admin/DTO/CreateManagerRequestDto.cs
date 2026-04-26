using System.ComponentModel.DataAnnotations;
namespace StudentPlanner.Core.Application.Admin.DTO;

public class CreateManagerRequestDto
{
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
    [Required] public Guid FacultyId { get; set; }
    [Required]
    [EmailAddress(ErrorMessage = "Email is not in a valid email format")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@pw\.edu\.pl$",
        ErrorMessage = "Registration is restricted to @pw.edu.pl email addresses.")]
    public string Email { get; set; } = null!;
}
