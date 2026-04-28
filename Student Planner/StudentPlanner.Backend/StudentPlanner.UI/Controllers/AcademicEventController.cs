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
    /// Retrieves academic events accessible to the authenticated user.
    /// </summary>
    /// <remarks>
    /// Non-admin users receive events associated only with their faculty.
    /// Admin users may retrieve all events or optionally restrict results
    /// to one or more faculties using the <c>facultyIds</c> query parameter.
    /// </remarks>
    /// <param name="facultyIds">
    /// Optional administrator-only faculty filters. When omitted, administrators
    /// receive all events. Non-admin users may not supply this parameter.
    /// </param>
    /// <returns>
    /// A collection of academic events visible to the authenticated user.
    /// </returns>
    /// <response code="200">Returns the requested academic events.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">
    /// If a non-admin user attempts to filter events by faculty.
    /// </response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAccessibleEvents([FromQuery] List<Guid> facultyIds)
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (id == null || role == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        if (role != UserRoleOptions.Admin.ToString() && facultyIds.Any())
            return Forbid();

        try
        {
            var result = await _academicEventService.GetAccessibleEventsAsync(Guid.Parse(id), role, facultyIds);
            return Ok(result);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving events." });
        }
    }

    /// <summary>
    /// Retrieves details of a specific academic event by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the academic event.</param>
    /// <returns>The event details if found; otherwise, Not Found.</returns>
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

    /// <summary>
    /// Subscribes the authenticated user to a specific academic event.
    /// </summary>
    /// <param name="id">The unique identifier of the academic event.</param>
    /// <returns>No Content if the subscription succeeds; otherwise, Not Found.</returns>
    [Authorize]
    [HttpPut("{id:guid}/subscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Subscribe(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            await _academicEventService.SubscribeAsync(id, Guid.Parse(userId));
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while subscribing to event." });
        }
    }

    /// <summary>
    /// Unsubscribes the authenticated user from a specific academic event.
    /// </summary>
    /// <param name="id">The unique identifier of the academic event.</param>
    /// <returns>No Content if the unsubscription succeeds; otherwise, Not Found.</returns>
    [Authorize]
    [HttpPut("{id:guid}/unsubscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            await _academicEventService.UnsubscribeAsync(id, Guid.Parse(userId));
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while unsubscribing from event." });
        }
    }
}