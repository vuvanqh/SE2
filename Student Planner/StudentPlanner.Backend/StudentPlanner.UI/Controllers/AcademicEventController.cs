using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.AcademicEvents.ServiceContracts;
using StudentPlanner.Core.Entities;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Controller for managing academic events.
/// </summary>
[Route("api/academic-events")]
[ApiController]
public class AcademicEventController : ControllerBase
{
    private readonly IAcademicEventService _academicEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcademicEventController"/> class.
    /// </summary>
    /// <param name="academicEventService">The academic event service.</param>
    public AcademicEventController(IAcademicEventService academicEventService)
    {
        _academicEventService = academicEventService;
    }

    /// <summary>
    /// Retrieves all academic events in the system. Accessible only by Admins.
    /// </summary>
    /// <returns>A list of all academic events.</returns>
    [Authorize(Roles = nameof(UserRoleOptions.Admin))]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllEvents()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null)
            return Unauthorized(new { Message = "Unauthorized access" });
        try
        {
            var result = await _academicEventService.GetAllEventsAsync(Guid.Parse(id));
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving events." });
        }
    }

    /// <summary>
    /// Retrieves all academic events relevant to the authenticated user's faculty.
    /// </summary>
    /// <returns>A list of academic events for the user's faculty.</returns>
    [Authorize]
    [HttpGet("faculty")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFacultyEvents()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var result = await _academicEventService.GetEventsForUserAsync(Guid.Parse(userId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving faculty events." });
        }
    }

    /// <summary>
    /// Retrieves details of a specific academic event by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the academic event.</param>
    /// <returns>The event details if found; otherwise, Not Found.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventDetails(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var result = await _academicEventService.GetEventByIdAsync(id, Guid.Parse(userId));
            if (result == null)
                return NotFound(new { Message = "Event not found." });

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving event details." });
        }
    }
}
