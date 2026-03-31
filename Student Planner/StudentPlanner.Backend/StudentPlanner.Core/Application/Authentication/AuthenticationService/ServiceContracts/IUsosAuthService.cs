namespace StudentPlanner.Core.Application.Authentication;

public interface IUsosAuthService
{
    Task<bool> LoginAsync(string email, string password);
}