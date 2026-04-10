using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Authentication;

public interface IIdentityService
{
    Task<User> SignInAsync(string email, string password);
    Task RegisterUser(User user, string password, Guid? facultyId, string? role = null);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPasswd);
    Task<IList<string>> GetUserRolesAsync(User user);
    Task UpdateToken(string email, string tokenHash, DateTime expirationDate, DateTime issuedAt);
    Task <User?> GetUserByIdAsync(Guid userId);
    Task<List<User>> GetAllUsersAsync();
    Task  DeleteUserAsync(Guid userId);
}
