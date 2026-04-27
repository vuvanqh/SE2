using StudentPlanner.Core.Application.AcademicEvents.DTOs;
using StudentPlanner.Core.Application.AcademicEvents.ServiceContracts;
using StudentPlanner.Core.Application.AcademicEvents.Mapping;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;

namespace StudentPlanner.Core.Application.AcademicEvents.Services;

public class AcademicEventService : IAcademicEventService
{
    private readonly IAcademicEventRepository _academicEventRepository;
    private readonly IUserRepository _userRepository;

    public AcademicEventService(IAcademicEventRepository academicEventRepository, IUserRepository userRepository)
    {
        _academicEventRepository = academicEventRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<AcademicEventResponse>> GetAccessibleEventsAsync(Guid id, string role, List<Guid>? facultyIds)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        IEnumerable<AcademicEvent> events;
        if (role != UserRoleOptions.Admin.ToString())
        {
            if (user.Faculty == null)
                return Enumerable.Empty<AcademicEventResponse>();
            events = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
        }
        else
        {
            events = facultyIds != null && facultyIds.Count > 0 ?
                await _academicEventRepository.GetByFacultiesAsync(facultyIds) :
                await _academicEventRepository.GetAllAsync();
        }

        var subscribedEventIds = await _academicEventRepository.GetSubscribedEventIdsAsync(id);
        return events.Select(e => e.ToAcademicEventResponse(subscribedEventIds.Contains(e.Id)));
    }

    public async Task<AcademicEventResponse?> GetEventByIdAsync(Guid id, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var e = await _academicEventRepository.GetByIdAsync(id);
        if (e == null) return null;

        if ((user.Role != UserRoleOptions.Admin.ToString()) && (user.Faculty == null || e.FacultyId != user.Faculty.Id))
        {
            // prevent showing events from other faculties
            return null;
        }

        var isSubscribed = await _academicEventRepository.IsSubscribedAsync(id, userId);
        return e.ToAcademicEventResponse(isSubscribed);
    }

    public async Task<IEnumerable<AcademicEventResponse>> GetEventsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (user.Faculty == null)
            return Enumerable.Empty<AcademicEventResponse>();

        var events = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
        var subscribedEventIds = await _academicEventRepository.GetSubscribedEventIdsAsync(userId);
        return events.Select(e => e.ToAcademicEventResponse(subscribedEventIds.Contains(e.Id)));
    }

    public async Task SubscribeAsync(Guid eventId, Guid userId)
    {
        await EnsureUserCanAccessEventAsync(eventId, userId);
        await _academicEventRepository.SubscribeAsync(eventId, userId);
    }

    public async Task UnsubscribeAsync(Guid eventId, Guid userId)
    {
        await EnsureUserCanAccessEventAsync(eventId, userId);

        bool isSubscribed = await _academicEventRepository.IsSubscribedAsync(eventId, userId);
        if (!isSubscribed)
            throw new KeyNotFoundException("Subscription not found.");

        await _academicEventRepository.UnsubscribeAsync(eventId, userId);
    }

    private async Task EnsureUserCanAccessEventAsync(Guid eventId, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var academicEvent = await _academicEventRepository.GetByIdAsync(eventId);
        if (academicEvent == null)
            throw new KeyNotFoundException("Event not found.");

        if (user.Role != UserRoleOptions.Admin.ToString()
            && (user.Faculty == null || academicEvent.FacultyId != user.Faculty.Id))
        {
            throw new KeyNotFoundException("Event not found.");
        }
    }
}
