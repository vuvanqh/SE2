using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Notifications.DTOs;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using System.Security.Claims;

namespace StudentPlanner.UI.Controllers;

/// <summary>
/// Controller for managing user notification preferences.
/// </summary>
[Route("api/notification-preferences")]
[ApiController]
[Authorize]
public class NotificationPreferencesController : ControllerBase
{
    private readonly INotificationPreferenceService _notificationPreferenceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferencesController"/> class.
    /// </summary>
    /// <param name="notificationPreferenceService">The notification preference service.</param>
    public NotificationPreferencesController(INotificationPreferenceService notificationPreferenceService)
    {
        _notificationPreferenceService = notificationPreferenceService;
    }

    /// <summary>
    /// Retrieves the notification preference for the authenticated user.
    /// </summary>
    /// <returns>The current notification preference.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPreference()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            var result = await _notificationPreferenceService.GetPreferenceAsync(Guid.Parse(userId));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the notification preference for the authenticated user.
    /// </summary>
    /// <param name="request">The notification preference update payload.</param>
    /// <returns>No Content if the preference is updated successfully.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreference(UpdateNotificationPreferenceRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Unauthorized access" });

            await _notificationPreferenceService.UpdatePreferenceAsync(Guid.Parse(userId), request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}