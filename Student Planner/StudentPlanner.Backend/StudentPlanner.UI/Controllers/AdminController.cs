using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Exceptions;
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
    /// <summary>
    /// Creates an Admin Controller which uses only adminService
    /// </summary>
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Deletes user Form of the LINK : admin/users/{userId(fromDb)}
    /// </summary>
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            await _adminService.DeleteUserAsync(userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Syncs users with the current Usos Active inactive structure on click 
    /// currently has an issue with UsosTokens as on outdate it will track them as 'bad'
    /// </summary>
    [HttpPost("users/sync")]
    public async Task<ActionResult<SyncUsersResultDto>> SyncUsers()
    {
        try
        {
            var result = await _adminService.SyncUsersWithUsosAsync();
            return Ok(result);
        }
        catch (UsosException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
    /// <summary>
    /// Creates and Registers Manager
    /// Body:
    /// {
    /// "firstName": NAme,
    ///"lastName": Surname",
    ///"facultyId": ID(used UsosId)
    /// }
    /// </summary>

    [HttpPost("managers")]
    public async Task<ActionResult<ManagerCreationResultDto>> CreateManager([FromBody] CreateManagerRequestDto request)
    {
        try
        {
            var result = await _adminService.CreateManagerAsync(request);
            return Ok(result);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("managers")]
    public async Task<ActionResult<ManagerResponseDto>> GetManagers()
    {
        try
        {
            var result = await _adminService.GetManagersAsync();
            return Ok(result);
        }
        catch(Exception ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }
    [HttpGet("users")]
    public async Task<ActionResult<UsersResultDto>> GetAllUsers()
    {
        try
        {
            var result = await _adminService.GetAllUsersAsync();
            return Ok(result);
        }
        catch(Exception ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }

}