using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using StudentPlanner.Core.Application.EventRequests;
using StudentPlanner.Core.Entities;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Controller for managing event requests.
/// </summary>
[Route("api/event-requests")]
[ApiController]
public class EventRequestController : ControllerBase
{
    private readonly IEventRequestService _eventRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventRequestController"/> class.
    /// </summary>
    /// <param name="eventRequestService">The event request service.</param>
    public EventRequestController(IEventRequestService eventRequestService)
    {
        _eventRequestService = eventRequestService;
    }

    /// <summary>
    /// Gets all event requests for the current user.
    /// </summary>
    /// <returns>A list of event requests for the authenticated user.</returns>
    [HttpGet]
    [Authorize(Roles = nameof(UserRoleOptions.Manager))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var response = await _eventRequestService.GetByManagerIdAsync(Guid.Parse(userId));
        return Ok(response);
    }

    /// <summary>
    /// Gets all event requests in the system.
    /// </summary>
    /// <returns>A list of all event requests.</returns>
    [HttpGet("all")]
    [Authorize(Roles = nameof(UserRoleOptions.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var response = await _eventRequestService.GetAllAsync();
        return Ok(response);
    }

    /// <summary>
    /// Gets an event request by its ID.
    /// </summary>
    /// <param name="requestId">The ID of the event request.</param>
    /// <returns>The event request details.</returns>
    [HttpGet("{requestId:guid}")]
    [Authorize(Roles = nameof(UserRoleOptions.Manager))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequestById(Guid requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        var response = await _eventRequestService.GetByIdAsync(Guid.Parse(userId), requestId);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new event request.
    /// </summary>
    /// <param name="request">The event request data to create.</param>
    /// <returns>The created event request ID and success message.</returns>
    [HttpPost("create")]
    [Authorize(Roles = nameof(UserRoleOptions.Manager))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest(CreateEventRequestRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access." });

        Guid requestId = await _eventRequestService.CreateAsync(Guid.Parse(userId), request);
        return Ok(new { EventRequestId = requestId, Message = "Success" });
    }

    /// <summary>
    /// Deletes an event request.
    /// </summary>
    /// <param name="requestId">The ID of the event request to delete.</param>
    /// <returns>A success message.</returns>
    [HttpDelete("delete/{requestId:guid}")]
    [Authorize(Roles = nameof(UserRoleOptions.Manager))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRequest(Guid requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access." });

        await _eventRequestService.DeleteAsync(Guid.Parse(userId), requestId);
        return Ok(new { Message = "Success" });
    }

    /// <summary>
    /// Approves an event request.
    /// </summary>
    /// <param name="requestId">The ID of the event request to approve.</param>
    /// <returns>A success message.</returns>
    [HttpPatch("approve/{requestId:guid}")]
    [Authorize(Roles = nameof(UserRoleOptions.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveRequest(Guid requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _eventRequestService.ApproveAsync(Guid.Parse(userId), requestId);
        return Ok(new { Message = "Success" });
    }

    /// <summary>
    /// Rejects an event request.
    /// </summary>
    /// <param name="requestId">The ID of the event request to reject.</param>
    /// <returns>A success message.</returns>
    [HttpPatch("reject/{requestId:guid}")]
    [Authorize(Roles = nameof(UserRoleOptions.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectRequest(Guid requestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _eventRequestService.RejectAsync(Guid.Parse(userId), requestId);
        return Ok(new { Message = "Success" });
    }
}
