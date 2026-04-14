using System;
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
    private readonly ILogger<EventRequestController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventRequestController"/> class.
    /// </summary>
    /// <param name="eventRequestService">The event request service.</param>
    /// <param name="logger">The logger instance.</param>
    public EventRequestController(IEventRequestService eventRequestService, ILogger<EventRequestController> logger)
    {
        _eventRequestService = eventRequestService;
        _logger = logger;
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var response = await _eventRequestService.GetByManagerIdAsync(Guid.Parse(userId));
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving your requests." });
        }
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var response = await _eventRequestService.GetAllAsync();
            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving all requests." });
        }
    }

    /// <summary>
    /// Gets an event request by its ID.
    /// </summary>
    /// <param name="requestId">The ID of the event request.</param>
    /// <returns>The event request details.</returns>
    [HttpGet("{requestId:guid}")]
    [Authorize(Roles = nameof(UserRoleOptions.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequestById(Guid requestId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var response = await _eventRequestService.GetByIdAsync(requestId);
            return Ok(response);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("exist", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving the request details." });
        }
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access." });

            Guid requestId = await _eventRequestService.CreateAsync(Guid.Parse(userId), request);
            return Ok(new { EventRequestId = requestId, Message = "Success" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while creating the event request." });
        }
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access." });

            await _eventRequestService.DeleteAsync(Guid.Parse(userId), requestId);
            return Ok(new { Message = "Success" });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("exist", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while deleting the event request." });
        }
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            _logger.LogInformation("Admin {AdminId} is attempting to approve event request {RequestId}", userId, requestId);

            await _eventRequestService.ApproveAsync(Guid.Parse(userId), requestId);

            _logger.LogInformation("Admin {AdminId} successfully approved event request {RequestId}", userId, requestId);
            return Ok(new { Message = "Success" });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("exist", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Admin {AdminId} attempted to approve non-existent event request {RequestId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Admin {AdminId} attempted to approve event request {RequestId} in an invalid state", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while Admin {AdminId} was approving event request {RequestId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while approving the event request." });
        }
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
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            _logger.LogInformation("Admin {AdminId} is attempting to reject event request {RequestId}", userId, requestId);

            await _eventRequestService.RejectAsync(Guid.Parse(userId), requestId);

            _logger.LogInformation("Admin {AdminId} successfully rejected event request {RequestId}", userId, requestId);
            return Ok(new { Message = "Success" });
        }
        catch (ArgumentException ex) when (ex.Message.Contains("exist", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Admin {AdminId} attempted to reject non-existent event request {RequestId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Admin {AdminId} attempted to reject event request {RequestId} in an invalid state", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while Admin {AdminId} was rejecting event request {RequestId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value, requestId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while rejecting the event request." });
        }
    }
}
