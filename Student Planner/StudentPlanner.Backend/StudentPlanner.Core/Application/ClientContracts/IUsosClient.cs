using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Application.ClientContracts.DTO;
namespace StudentPlanner.Core.Application.Authentication;

public interface IUsosClient
{
    Task<UsosLoginResponse> LoginAsync(string email, string password);
    Task<List<Faculty>> GetFacultiesAsync();
    Task<List<UsosEventResponseDto>> GetTimetableAsync(string usosToken, DateOnly start, int days);
}