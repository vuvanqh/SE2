using System.Security.Principal;
using StudentPlanner.Core.Application.Authentication;

namespace StudentPlanner.Core;
public class AdminService : IAdminService
{
    private readonly IIdentityService _identityService;
    public AdminService(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _identityService.GetUserByIdAsync(userId);
        if(user == null)
        {
            throw new KeyNotFoundException ("User is not found");
        }
        await _identityService.DeleteUserAsync(userId);
    }
}