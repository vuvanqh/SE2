using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core;
using StudentPlanner.Core.Application.Admin.DTO;

namespace StudentPlanner.UI.Controllers;
/// <summary>
/// Unified controller for admin functionality and previlage based abilities.
/// </summary>
[Route("api/admin")]
[ApiController]
[Authorize(Roles = nameof(UserRoleOptions.Admin))]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }
    
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _adminService.DeleteUserAsync(userId);
        return NoContent(); // can be Ok potentially
    }
    [HttpPost("users/sync")]
    public async Task<ActionResult<SyncUsersResultDto>> SyncUsers()
    {
        var result = await _adminService.SyncUsersWithUsosAsync();
        return Ok(result);
    }
}