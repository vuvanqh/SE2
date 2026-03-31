using Microsoft.EntityFrameworkCore;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace StudentPlanner.Infrastructure.Repositories;


//TO DO
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
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
        return user?.ToUser();

    }

    public async Task<User?> GetUserByRefreshToken(string token)
    {
        return (await _context.Users.FirstOrDefaultAsync(u => u.RefreshTokenHash == token))?.ToUser();
    }

    public async Task<List<User>> GetUserByRoleAsync(string role)
    {
        throw new NotImplementedException();
    }
}
