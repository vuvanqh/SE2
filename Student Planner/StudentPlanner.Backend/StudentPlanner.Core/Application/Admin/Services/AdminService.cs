using System.Security.Principal;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Admin.DTO;
using System.Security.AccessControl;
using StudentPlanner.Core.Application.ClientContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Application.Exceptions;
using StudentPlanner.Core.Domain.RepositoryContracts;
namespace StudentPlanner.Core;

public class AdminService : IAdminService
{
    private readonly IIdentityService _identityService;
    private readonly IUsosClient _usosClient;
    private readonly IFacultyRepository _facultyRepository;
    public AdminService(IIdentityService identityService, IUsosClient usosClient, IFacultyRepository facultyRepository)
    {
        _identityService = identityService;
        _usosClient = usosClient;
        _facultyRepository = facultyRepository;
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _identityService.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User is not found");
        }
        await _identityService.DeleteUserAsync(userId);
    }
    public async Task<SyncUsersResultDto> SyncUsersWithUsosAsync()
    {
        var results = new SyncUsersResultDto();
        var users = await _identityService.GetAllUsersAsync();
        foreach (var user in users)
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
                await DeleteUserAsync(user.Id);
                results.FailedUsersEmail.Add(user.Email);
                results.DisabledUsers++;
            }
            catch (UsosException ex) when (ShouldDeleteStudent(user, ex))
            {
                await _identityService.DeleteUserAsync(user.Id);

                results.FailedUsersEmail.Add(user.Email);
                results.DisabledUsers++;
            }
            catch (UsosException)
            {
                results.FailedChecks++;
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
        var oneStudent = facultyStudents.FirstOrDefault(s => string.Equals(s.UniversityEmail, user.Email, StringComparison.OrdinalIgnoreCase));
        if (oneStudent == null)
        {
            return false;
        }
        if (!string.IsNullOrWhiteSpace(oneStudent.Status) && !string.Equals(oneStudent.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
        //More checks if needed here

    }
    private static bool ShouldDeleteStudent(User user, UsosException ex)
    {
        if (!string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase))
            return false;

        if (ex.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            return false;

        return ex.ResponseError?.Contains("Student not found", StringComparison.OrdinalIgnoreCase) == true;
    }
    public async Task<ManagerCreationResultDto> CreateManagerAsync(CreateManagerRequestDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }


        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required.");

        var faculty = await _facultyRepository.GetFacultyByUsosIdAsync(request.FacultyId);
        if (faculty == null)
            throw new InvalidOperationException("Faculty not found.");

        var temporaryPassword = GenerateTemporaryPassword();

        var managerUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
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
    public async Task<List<ManagerResponseDto>> GetManagersAsync()
    {
        var users = await _identityService.GetAllUsersAsync();
        return users.Where(u => string.Equals(u.Role, UserRoleOptions.Manager.ToString(), StringComparison.OrdinalIgnoreCase))
        .Select(u => new ManagerResponseDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            FacultyCode = u.Faculty?.FacultyCode,
        }).ToList();
    }
    public async Task<List<UsersResultDto>> GetAllUsersAsync()
    {
        var users = await _identityService.GetAllUsersAsync();
        return users.Select(u => new UsersResultDto
        {
            FirstName = u.FirstName,
            LastName = u.LastName,
            UserRole = u.Role,
            Email = u.Email,
            FacultyCode = u.Faculty?.FacultyCode,
        }).ToList();
    }

}