using StudentPlanner.Core.Application.Admin.DTO;
namespace StudentPlanner.Core;

public interface IAdminService
{
    Task DeleteUserAsync(Guid userId);
    Task<SyncUsersResultDto> SyncUsersWithUsosAsync();
    Task<ManagerCreationResultDto> CreateManagerAsync(CreateManagerRequestDto request);
    Task<List<ManagerResponseDto>> GetManagersAsync();
}