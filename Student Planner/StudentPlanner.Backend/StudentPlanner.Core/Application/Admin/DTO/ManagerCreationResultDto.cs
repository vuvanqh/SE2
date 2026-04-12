namespace StudentPlanner.Core.Application.Admin.DTO;
public class ManagerCreationResultDto
{
    public string Email {get;set;} = null!;
    public string TemporaryPassword {get;set;} = null!;
    public string Role {get;set;} = null!;
}