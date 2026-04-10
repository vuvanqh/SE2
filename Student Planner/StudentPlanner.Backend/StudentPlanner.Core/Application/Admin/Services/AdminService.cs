using System.Security.Principal;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Admin.DTO;
using System.Security.AccessControl;
using StudentPlanner.Core.Entities;
namespace StudentPlanner.Core;
public class AdminService : IAdminService
{
    private readonly IIdentityService _identityService;
    private readonly IUsosClient _usosClient;
    public AdminService(IIdentityService identityService, IUsosClient usosClient)
    {
        _identityService = identityService;
        _usosClient = usosClient;
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _identityService.GetUserByIdAsync(userId);
        if(user == null)
        {
            throw new KeyNotFoundException ("User is not found");
        }
        await _identityService.DeleteUserAsync(userId);
    }
    public async Task<SyncUsersResultDto> SyncUsersWithUsosAsync()
    {
        var results = new SyncUsersResultDto();
        var users = await _identityService.GetAllUsersAsync();
        foreach(var user in users)
        {
            results.CheckedUsers++;
            try
            {
                bool Valid = await isUserValid(user);
                if (Valid)
                {
                    results.ValidUsers++;
                    continue;
                }
                //DisableLogic here
                results.DisabledUsers++;
            }
            catch
            {
                results.FailedChecks++;
            }
        }
            return results;
    }
    private async Task<bool> isUserValid(Entities.User user)
    {
        if (string.IsNullOrWhiteSpace(user.UsosToken))
            return false;

        if (user.Faculty == null || string.IsNullOrWhiteSpace(user.Faculty.FacultyId))
            return false;
        
        var facultyStudents = await _usosClient.GetStudentsByFacultyAsync(
            user.UsosToken,
            user.Faculty.FacultyId);
        var oneStudent = facultyStudents.FirstOrDefault(s=>string.Equals(s.UniversityEmail, user.Email, StringComparison.OrdinalIgnoreCase));
        if(oneStudent== null)
        {
            return false;
        }
         if (!string.IsNullOrWhiteSpace(oneStudent.Status) && !string.Equals(oneStudent.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
        //More checks if needed here
        
    }
}