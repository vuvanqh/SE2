using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace StudentPlanner.Infrastructure.Repositories;


//TO DO
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Task DeleteUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetFacultyUsersAsync(Guid facultyId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        ApplicationUser? user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        return await GetUserWithRole(user);

    }

    public async Task<User?> GetUserByRefreshToken(string token)
    {
        ApplicationUser? user = (await _context.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == token));

        return await GetUserWithRole(user);

    }

    public async Task<List<User>> GetUserByRoleAsync(string role)
    {
        throw new NotImplementedException();
    }

    private async Task<User?> GetUserWithRole(ApplicationUser? user)
    {
        if (user == null)
            return null;

        var resp = await _userManager.GetRolesAsync(user);
        return user.ToUser(resp[0]);
    }
}
