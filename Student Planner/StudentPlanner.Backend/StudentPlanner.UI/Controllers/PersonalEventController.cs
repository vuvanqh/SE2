using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.PersonalEvents;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Provides endpoints for managing personal events for authenticated students.
/// </summary>
/// <remarks>
/// All endpoints require the caller to be authenticated with the "Student" policy.
/// The user identifier is extracted from JWT claims.
/// </remarks>
[Route("api/personal-event")]
[Authorize("Student")]
[ApiController]
public class PersonalEventController : ControllerBase
{
    private readonly IPersonalEventService _personalEventService;
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalEventController"/> class.
    /// </summary>
    /// <param name="eventService">Service handling personal event business logic.</param>
    public PersonalEventController(IPersonalEventService eventService)
    {
        _personalEventService = eventService;
    }

    /// <summary>
    /// Retrieves all personal events for the authenticated user.
    /// </summary>
    /// <returns>A collection of personal events.</returns>
    /// <response code="200">Returns the list of personal events.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPersonalEvents()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var resp = await _personalEventService.GetEventsAsync(Guid.Parse(userId));
        return Ok(resp);
    }

    /// <summary>
    /// Retrieves details of a specific personal event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <returns>Detailed information about the specified event.</returns>
    /// <response code="200">Returns the event details.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the event does not exist or does not belong to the user.</response>
    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventDetails(Guid eventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var resp = await _personalEventService.GetEventByIdAsync(Guid.Parse(userId), eventId);
        if (resp == null)
            return NotFound(new { Message = "Event not found." });
        return Ok(resp);
    }

    /// <summary>
    /// Creates a new personal event for the authenticated user.
    /// </summary>
    /// <param name="request">The event creation payload.</param>
    /// <returns>The identifier of the newly created event.</returns>
    /// <response code="200">Event successfully created.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEvent(CreatePersonalEventRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var resp = await _personalEventService.CreatePersonalEventAsync(Guid.Parse(userId), request);
        return Ok(new { EventId = resp, Message = "Success" });
    }

    /// <summary>
    /// Deletes a specific personal event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to delete.</param>
    /// <returns>A confirmation message.</returns>
    /// <response code="200">Event successfully deleted.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the event does not exist or does not belong to the user.</response>
    [HttpDelete("delete/{eventId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid eventId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _personalEventService.DeletePersonalEventAsync(Guid.Parse(userId), eventId);
        return Ok(new { Message = "Success" });
    }

    /// <summary>
    /// Updates an existing personal event.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to update.</param>
    /// <param name="request">The updated event data.</param>
    /// <returns>A confirmation message.</returns>
    /// <response code="200">Event successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the event does not exist or does not belong to the user.</response>
    [HttpPut("update/{eventId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent([FromRoute] Guid eventId, UpdatePersonalEventRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _personalEventService.UpdatePersonalEventAsync(Guid.Parse(userId), eventId, request);
        return Ok(new { Message = "Success" });
    }
}
