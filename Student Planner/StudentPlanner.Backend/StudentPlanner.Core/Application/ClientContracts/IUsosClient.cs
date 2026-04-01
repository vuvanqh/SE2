using StudentPlanner.Core.Domain.Entities;

namespace StudentPlanner.Core.Application.Authentication;

public interface IUsosClient
{
    Task<UsosLoginResponse> LoginAsync(string email, string password);
    Task<List<Faculty>> GetFacultiesAsync();
}