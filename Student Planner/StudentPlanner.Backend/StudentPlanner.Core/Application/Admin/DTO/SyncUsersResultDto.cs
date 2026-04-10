namespace StudentPlanner.Core.Application.Admin.DTO;
public class SyncUsersResultDto
{
    public int CheckedUsers {get;set;}
    public int ValidUsers{get;set;}
    public int DisabledUsers{get;set;}
    public int FailedChecks{get;set;}
}