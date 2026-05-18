using StudentPlanner.Core.Application.Notifications.DTOs;
using StudentPlanner.Core.Application.Notifications.ServiceContracts;
using StudentPlanner.Core.Domain.RepositoryContracts;

namespace StudentPlanner.Core.Application.Notifications.Services;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IUserRepository _userRepository;

    public NotificationPreferenceService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<NotificationPreferenceResponse> GetPreferenceAsync(Guid userId)
    {
        var preference = await _userRepository.GetNotificationPreferenceAsync(userId);
        if (preference == null)
            throw new KeyNotFoundException("User not found.");

        return new NotificationPreferenceResponse
        {
            NotificationsEnabled = preference.Value
        };
    }


    public async Task UpdatePreferenceAsync(Guid userId, UpdateNotificationPreferenceRequest request)
    {
        await _userRepository.UpdateNotificationPreferenceAsync(userId, request.NotificationsEnabled);
    }

    public async Task<bool> AreNotificationsEnabledAsync(Guid userId)
    {
        var preference = await _userRepository.GetNotificationPreferenceAsync(userId);
        if (preference == null)
            throw new KeyNotFoundException("User not found.");

        return preference.Value;
    }
}