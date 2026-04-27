using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Application.Authentication;
namespace StudentPlanner.Core.Application.ClientContracts;


public interface IUsosClient
{
    Task<UsosLoginResponse> LoginAsync(string email, string password);
    Task<List<Faculty>> GetFacultiesAsync();
    Task<List<UsosEventResponseDto>> GetTimetableAsync(string usosToken, DateOnly start, int days);
    Task<List<UsosStudentDto>> GetStudentsByFacultyAsync(string usosToken, string facultyId);
    Task<UsosEventResponseDto> GetEventAsync(string usosToken, string eventId);
}