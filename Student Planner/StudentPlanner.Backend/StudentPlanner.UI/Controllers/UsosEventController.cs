using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Domain.RepositoryContracts;
using System.Security.Claims;
namespace StudentPlanner.UI.Controllers;
[Route("api/usos-events")]
[ApiController]
[Authorize(Roles = "Student")]
public class UsosEventsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUsosClient _usosClient;

    public UsosEventsController(IUserRepository userRepository, IUsosClient usosClient)
    {
        _userRepository = userRepository;
        _usosClient = usosClient;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEvents([FromQuery] string? start, [FromQuery] int days = 30)
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
        : DateOnly.Parse(start);

    var events = await _usosClient.GetTimetableAsync(user.UsosToken, parsedStart, days);

    return Ok(events);
    }
}