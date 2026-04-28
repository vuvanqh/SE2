namespace StudentPlanner.Core.Application.Notifications.DTOs;

public record UpdateNotificationPreferenceRequest
{
    public bool NotificationsEnabled { get; init; }
}
