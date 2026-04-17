using Microsoft.AspNetCore.SignalR;
using StudentPlanner.Core.Entities;
using System.Security.Claims;

namespace StudentPlanner.UI.Hubs;

/// <summary>
/// SignalR hub responsible for managing real-time connections
/// related to event request notifications.
/// </summary>
/// <remarks>
/// This hub assigns connected users to logical groups based on their role.
/// Users with the <see cref="UserRoleOptions.Admin"/> role are added to the "admins" group,
/// enabling them to receive administrative notifications such as event request updates.
/// 
/// Authentication is expected to be configured globally, and user roles are resolved
/// from JWT claims.
/// </remarks>
public class EventRequestHub : Hub
{
    /// <summary>
    /// Handles logic executed when a new client establishes a connection to the hub.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// If the connected user has an "Admin" role (resolved from claims),
    /// their connection is added to the "admins" SignalR group.
    /// This allows broadcasting administrative updates efficiently.
    /// </remarks>
    public override async Task OnConnectedAsync()
    {
        var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse<UserRoleOptions>(roleClaim, true, out var role) && role == UserRoleOptions.Admin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
    }
}
