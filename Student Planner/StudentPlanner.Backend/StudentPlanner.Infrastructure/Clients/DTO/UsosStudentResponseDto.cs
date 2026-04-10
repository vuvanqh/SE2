namespace StudentPlanner.Infrastructure.Clients.DTO;

public class UsosStudentResponseDto
{
    public string student_id { get; set; } = null!;
    public string first_name { get; set; } = null!;
    public string last_name { get; set; } = null!;
    public string faculty_id { get; set; } = null!;
    public string university_email { get; set; } = null!;
    public string status { get; set; } = null!;
}