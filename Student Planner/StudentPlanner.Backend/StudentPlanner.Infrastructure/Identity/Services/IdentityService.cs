using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure.IdentityEntities;
using System.Data;

namespace StudentPlanner.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserRepository _userRepository;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager, IUserRepository userRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
    }
    public async Task<User> SignInAsync(string email, string password)
    {
        ApplicationUser user = (await _userManager.Users
            .Include(u => u.Faculty)
            .FirstOrDefaultAsync(u => u.Email == email)) ?? throw new UnauthorizedAccessException("Invalid Credentials"); //i suppose we do not want to disclose whether an account associated with this email exists

        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid Credentials");

        var roles = await _userManager.GetRolesAsync(user);

        var roleName = roles.FirstOrDefault() ?? UserRoleOptions.Student.ToString();
        return user.ToUser(roleName);
    }

    public async Task RegisterUser(User user, string password, Guid? facultyId, string? role = null)
    {

        ApplicationUser appUser = new ApplicationUser()
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FacultyId = facultyId,
            UsosToken = user.UsosToken
        };

        var result = await _userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        if (role != null)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
            await _userManager.AddToRoleAsync(appUser, role);
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new InvalidOperationException("Invalid Operation");
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPasswd)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new InvalidOperationException("Invalid Operation");
        var result = await _userManager.ResetPasswordAsync(user, token, newPasswd);
        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }
    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        ApplicationUser appUser = (await _userManager.FindByEmailAsync(user.Email)) ?? throw new InvalidOperationException("User not found");
        return await _userManager.GetRolesAsync(appUser);
    }
    public async Task UpdateToken(string email, string tokenHash, DateTime expirationDate, DateTime issuedAt)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new InvalidOperationException("Invalid Operation");
        user.RefreshTokenHash = tokenHash;
        user.RefreshTokenExpirationDate = expirationDate;
        user.RefreshTokenIssuedAt = issuedAt;
        await _userManager.UpdateAsync(user);
    }
    public async Task UpdateUsosToken(string UsosToken, User user)
    {
        ApplicationUser appUser = (await _userManager.FindByEmailAsync(user.Email)) ?? throw new InvalidOperationException("User not found");
        appUser.UsosToken = UsosToken;
        await _userManager.UpdateAsync(appUser);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null)
        {
            return null;
        }
        var roles = await _userManager.GetRolesAsync(appUser);
        var roleName = roles.FirstOrDefault() ?? UserRoleOptions.Student.ToString();
        return appUser.ToUser(roleName);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var appUsers = await _userManager.Users.Include(u => u.Faculty).ToListAsync();
        var users = new List<User>();
        foreach (var appUser in appUsers)
        {
            var roles = await _userManager.GetRolesAsync(appUser);
            var roleName = roles.FirstOrDefault() ?? UserRoleOptions.Student.ToString();
            users.Add(appUser.ToUser(roleName));
        }
        return users;
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId.ToString());
        if (appUser == null)
            throw new KeyNotFoundException("User not found!");
        var roles = await _userManager.GetRolesAsync(appUser);
        var roleName = roles[0];
        await _userRepository.DeleteUserAsync(appUser.ToUser(roleName));
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser == null)
        {
            return null;
        }
        var roles = await _userManager.GetRolesAsync(appUser);
        var roleName = roles.FirstOrDefault() ?? UserRoleOptions.Student.ToString();
        return appUser.ToUser(roleName);
    }

    public async Task LogOut(string id)
    {
        var appUser = await _userManager.FindByIdAsync(id);
        if (appUser == null)
            throw new KeyNotFoundException("User not found!");

        appUser.RefreshTokenHash = null;
        await _signInManager.SignOutAsync();
    }
}
