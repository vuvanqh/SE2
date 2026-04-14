using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Application.Exceptions;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Application.Events.UsosEvents.ServiceContracts;
using System.Security.Claims;
using System.Globalization;
namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Provides endpoints for fetching time-table for  students.
/// </summary>

[Route("api/usos-events")]
[ApiController]
[Authorize(Roles = "Student")]

public class UsosEventsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUsosEventService _usosEventService;
    /// <summary>
    /// Creates a controller, it has usosCLient and userRepository.
    /// </summary>

    public UsosEventsController(IUserRepository userRepository, IUsosEventService usosEventService)
    {
        _userRepository = userRepository;
        _usosEventService = usosEventService;
    }

    /// <summary>
    /// api end point of the form  PortNum/api/usos-events/me?start=yyyy-mm-day andpersand days=n
    /// </summary>

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEvents([FromQuery] string? start, [FromQuery] int days = 30)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "User id claim not found." });

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user id claim format." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            if (string.IsNullOrWhiteSpace(user.UsosToken))
                return BadRequest(new { message = "User does not have a linked USOS token." });

            var parsedStart = string.IsNullOrWhiteSpace(start)
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : DateOnly.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                
                var events = await _usosEventService.SyncAndGetEventsAsync(userId, user.UsosToken, parsedStart, days);

            return Ok(events);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Invalid start date format. Use yyyy-MM-dd." });
        }
        catch (UsosException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (InvalidResponseException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
}