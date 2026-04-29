using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<User>> GetFacultyUsersAsync(Guid? facultyId);
    Task<List<User>> GetUserByRoleAsync(string role);
    Task DeleteUserAsync(User user);
    Task<User?> GetUserByRefreshToken(string token);
    Task<User?> GetByIdAsync(Guid userId);
    Task<bool?> GetNotificationPreferenceAsync(Guid userId);
    Task UpdateNotificationPreferenceAsync(Guid userId, bool notificationsEnabled);
}
