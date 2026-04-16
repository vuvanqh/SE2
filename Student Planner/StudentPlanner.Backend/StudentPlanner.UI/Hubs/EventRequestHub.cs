using Microsoft.AspNetCore.SignalR;
using StudentPlanner.Core.Entities;
using System.Security.Claims;

namespace StudentPlanner.UI.Hubs;

public class EventRequestHub: Hub
{
    private readonly ILogger<EventRequestHub> _logger;
    public EventRequestHub(ILogger<EventRequestHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogCritical($"Logging: {roleClaim}");

        if (Enum.TryParse<UserRoleOptions>(roleClaim, true, out var role) && role == UserRoleOptions.Admin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
    }
}
