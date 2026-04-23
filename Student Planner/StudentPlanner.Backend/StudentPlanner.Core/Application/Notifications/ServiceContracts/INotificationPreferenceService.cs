using StudentPlanner.Core.Application.Notifications.DTOs;

namespace StudentPlanner.Core.Application.Notifications.ServiceContracts;

public interface INotificationPreferenceService
{
    Task<NotificationPreferenceResponse> GetPreferenceAsync(Guid userId);
    Task UpdatePreferenceAsync(Guid userId, UpdateNotificationPreferenceRequest request);
    Task<bool> AreNotificationsEnabledAsync(Guid userId);
}