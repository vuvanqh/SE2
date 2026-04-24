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
/// Provides endpoints for retrieving USOS timetable events
/// for the currently authenticated student.
/// </summary>

[Route("api/usos-events")]
[ApiController]
[Authorize(Roles = "Student")]

public class UsosEventsController : ControllerBase
{
    private readonly IUsosEventService _usosEventService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsosEventsController"/> class.
    /// </summary>
    /// <param name="usosEventService">
    /// Service responsible for retrieving and synchronizing
    /// the authenticated student's USOS events.
    /// </param>
    public UsosEventsController(IUsosEventService usosEventService)
    {
        _usosEventService = usosEventService;
    }

    /// <summary>
    /// Retrieves the authenticated student's USOS timetable events
    /// for the specified date range.
    /// </summary>
    /// <param name="start">
    /// Optional start date in <c>yyyy-MM-dd</c> format.
    /// If not provided, the current UTC date is used.
    /// </param>
    /// <param name="days">
    /// Number of days to retrieve starting from <paramref name="start"/>.
    /// The default value is 30.
    /// </param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the list of synchronized USOS events.
    /// </returns>
    /// <response code="200">
    /// Returns the list of USOS events for the authenticated student.
    /// </response>
    /// <response code="400">
    /// Returned when the <paramref name="start"/> date format is invalid.
    /// </response>
    /// <response code="401">
    /// Returned when the user identifier claim is missing or invalid.
    /// </response>
    /// <response code="502">
    /// Returned when communication with the external USOS system fails
    /// or the response from USOS is invalid.
    /// </response>

    [HttpGet]
    public async Task<IActionResult> GetMyEvents([FromQuery] string? start, [FromQuery] int days = 30)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "User id claim not found." });

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid user id claim format." });

            var parsedStart = string.IsNullOrWhiteSpace(start)
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : DateOnly.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var events = await _usosEventService.SyncAndGetEventsAsync(userId, parsedStart, days);

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
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}