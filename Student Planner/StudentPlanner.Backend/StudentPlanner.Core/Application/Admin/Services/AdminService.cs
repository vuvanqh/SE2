using System.Security.Principal;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Admin.DTO;
using System.Security.AccessControl;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
namespace StudentPlanner.Core;
public class AdminService : IAdminService
{
    private readonly IIdentityService _identityService;
    private readonly IUsosClient _usosClient;
    private readonly IFacultyRepository _facultyRepository;
    public AdminService(IIdentityService identityService, IUsosClient usosClient,IFacultyRepository facultyRepository)
    {
        _identityService = identityService;
        _usosClient = usosClient;
        _facultyRepository = facultyRepository; 
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
                results.FailedUsersEmail.Add(user.Email);
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

        if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
        return true;
        }

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
    public async  Task<ManagerCreationResultDto> CreateManagerAsync(CreateManagerRequestDto request)
    {
        if (request == null){
        throw new ArgumentNullException(nameof(request));}

    var temporaryEmail = GenerateEmail();
    if (string.IsNullOrWhiteSpace(request.FirstName))
        throw new ArgumentException("First name is required.");

    if (string.IsNullOrWhiteSpace(request.LastName))
        throw new ArgumentException("Last name is required.");

    var existingUsers = await _identityService.GetAllUsersAsync();
    if (CheckExistenceOfUser(existingUsers, temporaryEmail)!)
        throw new InvalidOperationException("A user with this email already exists.");

    var faculty = await _facultyRepository.GetFacultyByUsosIdAsync(request.FacultyId);
    if (faculty == null)
        throw new InvalidOperationException("Faculty not found.");

    var temporaryPassword = GenerateTemporaryPassword();

    var managerUser = new User
    {
        Id = Guid.NewGuid(),
        Email = temporaryEmail,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Role = UserRoleOptions.Manager.ToString(),
        Faculty = faculty,
        UsosToken = null
    };

    await _identityService.RegisterUser(
        managerUser,
        temporaryPassword,
        faculty.Id,
        UserRoleOptions.Manager.ToString());

    return new ManagerCreationResultDto
    {
        Email = managerUser.Email,
        TemporaryPassword = temporaryPassword,
        Role = UserRoleOptions.Manager.ToString()
    };
        
    }
    private static string GenerateTemporaryPassword()
{
    const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    const string lower = "abcdefghijkmnopqrstuvwxyz";
    const string digits = "23456789";
    const string special = "!@#$%^&*";
    const string all = upper + lower + digits + special;

    var random = new Random();

    var chars = new List<char>
    {
        upper[random.Next(upper.Length)],
        lower[random.Next(lower.Length)],
        digits[random.Next(digits.Length)],
        special[random.Next(special.Length)]
    };

    for (int i = chars.Count; i < 12; i++)
    {
        chars.Add(all[random.Next(all.Length)]);
    }

    return new string(chars.OrderBy(_ => random.Next()).ToArray());
}

    private static string GenerateEmail()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghjklmnpqrstuvwxyz";
        const string digits = "1234567890";
        const string tail = "@pw.edu.pl";
        const string all = upper+lower+digits;
        var random = new Random();
        var starter = new List<char>
        {
            upper[random.Next(upper.Length)],
            lower[random.Next(lower.Length)],
            digits[random.Next(digits.Length)]
        };
        for(int i = starter.Count; i < 12; i++)
        {
            starter.Add(all[random.Next(all.Length)]);
        }
        return new string(starter.ToArray()) + tail;

    }
    private static bool CheckExistenceOfUser(List<User>existingUsers, string temporaryEmail)
    {
        return existingUsers.Any(user => user.Email == temporaryEmail);
    }
}