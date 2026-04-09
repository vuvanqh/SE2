namespace StudentPlanner.Core;
public interface IAdminService
{
    Task DeleteUserAsync(Guid userId);
}