using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Events;
using StudentPlanner.Core.Application.Events.EventPreveiws;
using StudentPlanner.Core.Entities;
using StudentPlanner.Core.Application.Exceptions;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Provides endpoints for retrieving event previews available to the authenticated user.
/// </summary>
/// <remarks>
/// This controller resolves the user identity and role from JWT claims
/// and returns a filtered set of event previews based on optional date range parameters.
/// </remarks>
[Route("api/event-preview")]
[ApiController]
public class EventPreviewController : ControllerBase
{
    private readonly IEventPreviewService _eventPreviewService;
    /// <summary>
    /// Initializes a new instance of the <see cref="EventPreviewController"/> class.
    /// </summary>
    /// <param name="eventPreviewService">
    /// Service responsible for retrieving event previews based on user context.
    /// </param>
    public EventPreviewController(IEventPreviewService eventPreviewService)
    {
        _eventPreviewService = eventPreviewService;
    }


    /// <summary>
    /// Retrieves event previews for the authenticated user.
    /// </summary>
    /// <param name="from">
    /// Optional start date filter. Only events occurring after this date are included.
    /// </param>
    /// <param name="days">
    /// Optional days indicates the range date starting from the starting ending on start date + days, from which events are to be included.
    /// </param>
    /// /// <param name="facultyIds">
    /// Optional faculty filter. Only events of faculties with faculty ids included in the parameter are returned.
    /// </param>
    /// <returns>A collection of event previews matching the specified criteria.</returns>
    /// <response code="200">Returns the list of event previews.</response>
    /// <response code="400">If the user role is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    public async Task<IActionResult> GetPreviews(DateTime? from, int? days, [FromQuery] List<Guid> facultyIds)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userId == null || role == null)
            return Unauthorized();
        if (role != UserRoleOptions.Admin.ToString() && (facultyIds?.Any() ?? false))
            return Forbid();

        if (!Enum.TryParse<UserRoleOptions>(role, true, out var parsedRole))
            return BadRequest("Invalid role");

        try
        {
            var result = await _eventPreviewService.GetForUserAsync(new UserContext { Id = Guid.Parse(userId), Role = parsedRole }, new EventPreviewQuery { From = from, Days = days, FacultyIds = facultyIds });
            return Ok(result);
        }
        catch (UsosException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "USOS_AUTH_REQUIRED" });
            }
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while retrieving previews.", Details = ex.Message });
        }
    }
}
