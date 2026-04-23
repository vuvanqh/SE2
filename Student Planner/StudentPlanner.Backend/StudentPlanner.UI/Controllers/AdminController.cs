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
/// Provides administrative endpoints for managing users, managers, and synchronization operations.
/// Access to all actions in this controller is restricted to users with the Admin role.
/// </summary>
[Route("api/admin")]
[ApiController]
[Authorize(Roles = nameof(UserRoleOptions.Admin))]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminController"/> class.
    /// </summary>
    /// <param name="adminService">
    /// Service responsible for handling administrative operations.
    /// </param>
    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user to delete.
    /// </param>
    /// <returns>
    /// Returns <see cref="NoContentResult"/> if the user was successfully deleted.
    /// </returns>
    /// <response code="204">The user was successfully deleted.</response>
    /// <response code="404">The specified user was not found.</response>
    /// <response code="401">The request is unauthorized.</response>
    /// <response code="403">The authenticated user does not have Admin privileges.</response>
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
    /// Synchronizes users in the local database with the current data retrieved from the USOS system.
    /// </summary>
    /// <remarks>
    /// This operation validates stored users against the external USOS source and updates the local state accordingly.
    /// </remarks>
    /// <returns>
    /// A summary describing how many users were checked, validated, disabled, or failed during synchronization.
    /// </returns>
    /// <response code="200">The synchronization completed successfully.</response>
    /// <response code="502">The application could not communicate correctly with the external USOS service.</response>
    /// <response code="401">The request is unauthorized.</response>
    /// <response code="403">The authenticated user does not have Admin privileges.</response>
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
    /// Creates a new manager account.
    /// </summary>
    /// <param name="request">
    /// The data required to create a manager, including first name, last name, and faculty identifier.
    /// </param>
    /// <returns>
    /// The created manager's generated credentials and assigned role.
    /// </returns>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/admin/managers
    ///     {
    ///         "firstName": "John",
    ///         "lastName": "Smith",
    ///         "facultyId": "MINI"
    ///         "email": "whateveremail@pw.edu.pl"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">The manager was created successfully.</response>
    /// <response code="400">The request data is invalid or the faculty does not exist.</response>
    /// <response code="401">The request is unauthorized.</response>
    /// <response code="403">The authenticated user does not have Admin privileges.</response>

    [HttpPost("managers")]
    public async Task<ActionResult<ManagerCreationResultDto>> CreateManager([FromBody] CreateManagerRequestDto request)
    {
        try
        {
            var result = await _adminService.CreateManagerAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Retrieves all managers from the database.
    /// </summary>
    /// <returns>
    /// A collection of manager records.
    /// </returns>
    /// <response code="200">The list of managers was retrieved successfully.</response>
    /// <response code="400">The request could not be processed.</response>
    /// <response code="401">The request is unauthorized.</response>
    /// <response code="403">The authenticated user does not have Admin privileges.</response>
    [HttpGet("managers")]
    public async Task<ActionResult<ManagerResponseDto>> GetManagers()
    {
        try
        {
            var result = await _adminService.GetManagersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Retrieves all users from the database.
    /// </summary>
    /// <returns>
    /// A result object containing all users available to the administrator.
    /// </returns>
    /// <response code="200">The list of users was retrieved successfully.</response>
    /// <response code="400">The request could not be processed.</response>
    /// <response code="401">The request is unauthorized.</response>
    /// <response code="403">The authenticated user does not have Admin privileges.</response>
    [HttpGet("users")]
    public async Task<ActionResult<UsersResultDto>> GetAllUsers()
    {
        try
        {
            var result = await _adminService.GetAllUsersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}