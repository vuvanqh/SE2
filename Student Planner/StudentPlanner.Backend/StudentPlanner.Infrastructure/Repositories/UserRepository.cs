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

    public async Task DeleteUserAsync(User user)
    {
        var tsx = await _context.Database.BeginTransactionAsync();
        try
        {
            var u = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (u == null)
                return;
            if (user.Role == "Manager")
            {
                var requests = await _context.EventRequests.Where(r => r.ManagerId == user.Id).ToListAsync();
                _context.EventRequests.RemoveRange(requests);
            }
            _context.Users.Remove(u);
            await _context.SaveChangesAsync();
            await tsx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tsx.RollbackAsync();
            throw new DbUpdateException("Error deleting user: " + ex.Message);
        }
    }

    public Task<List<User>> GetFacultyUsersAsync(Guid facultyId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        ApplicationUser? user = await _context.Users.Include(u => u.Faculty).FirstOrDefaultAsync(user => user.Email == email);
        return await GetUserWithRole(user);
    }

    public async Task<User?> GetUserByRefreshToken(string token)
    {
        ApplicationUser? user = (await _context.Users.Include(u => u.Faculty).FirstOrDefaultAsync(u => u.RefreshTokenHash == token));

        return await GetUserWithRole(user);

    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        ApplicationUser? user = await _context.Users.Include(u => u.Faculty).FirstOrDefaultAsync(u => u.Id == userId);
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

    public async Task<bool?> GetNotificationPreferenceAsync(Guid userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.NotificationsEnabled;
    }

    public async Task UpdateNotificationPreferenceAsync(Guid userId, bool notificationsEnabled)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.NotificationsEnabled = notificationsEnabled;
        await _context.SaveChangesAsync();
    }
}
