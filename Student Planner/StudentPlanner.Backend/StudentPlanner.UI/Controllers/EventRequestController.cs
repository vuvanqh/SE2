using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using StudentPlanner.Core.Application.EventRequests;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

[Route("api/event-requests")]
[ApiController]
public class EventRequestController : ControllerBase
{
    private readonly IEventRequestService _eventRequestService;

    public EventRequestController(IEventRequestService eventRequestService)
    {
        _eventRequestService = eventRequestService;
    }

    [HttpGet]
    [Authorize]
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

    [HttpGet("all")]
    [Authorize]
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

    [HttpGet("{requestId:guid}")]
    [Authorize]
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

    // Manager endpoints
    [HttpPost("create")]
    [Authorize]
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

    // Manager endpoints
    [HttpDelete("delete/{requestId:guid}")]   
    [Authorize]
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

    // Admin endpoints
    [HttpPatch("approve/{requestId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveRequest(Guid requestId, ReviewEventRequestRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _eventRequestService.ApproveAsync(Guid.Parse(userId), requestId, request);
        return Ok(new { Message = "Success" });
    }

    // Admin endpoints
    [HttpPatch("reject/{requestId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectRequest(Guid requestId, ReviewEventRequestRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized(new { Message = "Unauthorized access" });

        await _eventRequestService.RejectAsync(Guid.Parse(userId), requestId, request);
        return Ok(new { Message = "Success" });
    }
}