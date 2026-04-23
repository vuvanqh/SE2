namespace StudentPlanner.Core.Application.Notifications.DTOs;

public record NotificationPreferenceResponse
{
    public bool NotificationsEnabled { get; init; }
}
