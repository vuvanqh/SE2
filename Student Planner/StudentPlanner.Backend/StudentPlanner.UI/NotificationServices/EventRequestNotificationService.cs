using Microsoft.AspNetCore.SignalR;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.Core.Entities;
using StudentPlanner.UI.Hubs;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;

namespace StudentPlanner.UI.NotificationServices;

/// <summary>
/// Service responsible for dispatching real-time notifications
/// related to event request state changes.
/// </summary>
/// <remarks>
/// This service uses SignalR to notify:
/// <list type="bullet">
/// <item>
/// <description>Administrators via the "admins" group.</description>
/// </item>
/// <item>
/// <description>Specific users via their unique user identifier.</description>
/// </item>
/// </list>
/// 
/// It abstracts the underlying SignalR hub communication, allowing application
/// services to trigger notifications without direct dependency on SignalR APIs.
/// </remarks>
public class EventRequestNotificationService : IEventRequestNotificationService
{
    private readonly IHubContext<EventRequestHub> _hub;
    private readonly INotificationPreferenceService _notificationPreferenceService;
    /// <summary>
    /// Initializes a new instance of the <see cref="EventRequestNotificationService"/> class.
    /// </summary>
    /// <param name="hub">
    /// SignalR hub context used to send messages to connected clients.
    /// </param>
    public EventRequestNotificationService(IHubContext<EventRequestHub> hub, INotificationPreferenceService notificationPreferenceService)
    {
        _hub = hub;
        _notificationPreferenceService = notificationPreferenceService;
    }

    /// <summary>
    /// Notifies relevant parties that an event request has been updated.
    /// </summary>
    /// <param name="managerId">
    /// The unique identifier of the manager associated with the event request.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method sends a "refreshEventRequests" message to:
    /// <list type="bullet">
    /// <item>
    /// <description>All administrators (via the "admins" group).</description>
    /// </item>
    /// <item>
    /// <description>The specific manager (via their user identifier).</description>
    /// </item>
    /// </list>
    /// 
    /// The client application is expected to handle this message
    /// and refresh its event request data accordingly.
    /// </remarks>
    public async Task EventRequestUpdated(Guid managerId)
    {
        await _hub.Clients.Group("admins").SendAsync("refreshEventRequests");
        if (await _notificationPreferenceService.AreNotificationsEnabledAsync(managerId))
        {
            await _hub.Clients.User(managerId.ToString()).SendAsync("refreshEventRequests");
        }
    }

    /// <summary>
    /// Notifies administrators that the event request list has changed.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method broadcasts a "refreshEventRequests" message
    /// to all users in the "admins" group.
    /// It is typically used when event requests are created or deleted.
    /// </remarks>
    public async Task NotifyEventRequestListChanged()
    {
        await _hub.Clients.Group("admins").SendAsync("refreshEventRequests");
    }
}
