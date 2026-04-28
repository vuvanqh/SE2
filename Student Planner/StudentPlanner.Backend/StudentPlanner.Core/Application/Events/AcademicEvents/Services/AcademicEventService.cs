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
    private readonly IFacultyRepository _facultyRepository;

    public AcademicEventService(IAcademicEventRepository academicEventRepository, IUserRepository userRepository, IFacultyRepository facultyRepository)
    {
        _academicEventRepository = academicEventRepository;
        _userRepository = userRepository;
        _facultyRepository = facultyRepository;
    }

    private async Task<Dictionary<Guid, string>> GetFacultyNamesAsync(IEnumerable<AcademicEvent> events)
    {
        var facultyIds = events.Select(e => e.FacultyId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (!facultyIds.Any()) return new Dictionary<Guid, string>();

        var faculties = await _facultyRepository.GetAllFacultiesAsync();
        if (faculties == null) return new Dictionary<Guid, string>();

        return faculties.Where(f => facultyIds.Contains(f.Id)).ToDictionary(f => f.Id, f => f.FacultyName);
    }

    public async Task<IEnumerable<AcademicEventResponse>> GetAccessibleEventsAsync(Guid id, string role, List<Guid>? facultyIds)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        IEnumerable<AcademicEvent> events = Enumerable.Empty<AcademicEvent>();
        if (role == UserRoleOptions.Admin.ToString())
        {
            if (facultyIds == null || !facultyIds.Any())
            {
                events = await _academicEventRepository.GetAllAsync();
            }
            else
            {
                var hasUniversity = facultyIds.Contains(Guid.Empty);
                var actualFacultyIds = facultyIds.Where(id => id != Guid.Empty).ToList();
                
                var facultyEvents = actualFacultyIds.Any() 
                    ? await _academicEventRepository.GetByFacultiesAsync(actualFacultyIds)
                    : Enumerable.Empty<AcademicEvent>();
                
                var universityEvents = hasUniversity 
                    ? await _academicEventRepository.GetUniversityEventsAsync()
                    : Enumerable.Empty<AcademicEvent>();

                events = facultyEvents.Concat(universityEvents);
            }
        }
        else if (role == UserRoleOptions.Student.ToString())
        {
            var universityEvents = await _academicEventRepository.GetUniversityEventsAsync();
            if (user.Faculty == null)
            {
                events = universityEvents;
            }
            else
            {
                var facultyEvents = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
                events = facultyEvents.Concat(universityEvents);
            }
        }
        else // Manager
        {
            if (user.Faculty == null)
            {
                // University Manager sees University Events only
                events = await _academicEventRepository.GetUniversityEventsAsync();
            }
            else
            {
                // Faculty Manager sees Faculty Events only
                events = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
            }
        }

        var subscribedEventIds = await _academicEventRepository.GetSubscribedEventIdsAsync(id);
        var facultyNames = await GetFacultyNamesAsync(events);
        
        return events.Select(e => e.ToAcademicEventResponse(
            e.FacultyId.HasValue && facultyNames.TryGetValue(e.FacultyId.Value, out var name) ? name : null,
            subscribedEventIds.Contains(e.Id)));
    }

    public async Task<AcademicEventResponse?> GetEventByIdAsync(Guid id, Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var e = await _academicEventRepository.GetByIdAsync(id);
        if (e == null) return null;

        if (user.Role != UserRoleOptions.Admin.ToString())
        {
            if (user.Role == UserRoleOptions.Student.ToString())
            {
                if (e is FacultyEvent fe && (user.Faculty == null || fe.FacultyId != user.Faculty.Id))
                {
                    return null;
                }
                // Students can always see UniversityEvents
            }
            else // Manager
            {
                if (e is UniversityEvent && user.Faculty != null)
                {
                    // Faculty Manager cannot see University Events
                    return null;
                }
                if (e is FacultyEvent fe && (user.Faculty == null || fe.FacultyId != user.Faculty.Id))
                {
                    // University Manager cannot see Faculty Events
                    // OR Faculty Manager cannot see other faculty events
                    return null;
                }
            }
        }

        var isSubscribed = await _academicEventRepository.IsSubscribedAsync(id, userId);
        string? facultyName = null;
        if (e.FacultyId.HasValue)
        {
            var faculty = await _facultyRepository.GetFacultyByIdAsync(e.FacultyId.Value);
            facultyName = faculty?.FacultyName;
        }

        return e.ToAcademicEventResponse(facultyName, isSubscribed);
    }

    public async Task<IEnumerable<AcademicEventResponse>> GetEventsForUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        IEnumerable<AcademicEvent> events = Enumerable.Empty<AcademicEvent>();
        if (user.Role == UserRoleOptions.Admin.ToString())
        {
            events = await _academicEventRepository.GetAllAsync();
        }
        else if (user.Role == UserRoleOptions.Student.ToString())
        {
            var universityEvents = await _academicEventRepository.GetUniversityEventsAsync();
            if (user.Faculty == null)
            {
                events = universityEvents;
            }
            else
            {
                var facultyEvents = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
                events = facultyEvents.Concat(universityEvents);
            }
        }
        else // Manager
        {
            if (user.Faculty == null)
            {
                events = await _academicEventRepository.GetUniversityEventsAsync();
            }
            else
            {
                events = await _academicEventRepository.GetByFacultyIdAsync(user.Faculty.Id);
            }
        }

        var subscribedEventIds = await _academicEventRepository.GetSubscribedEventIdsAsync(userId);

        if (user.Role == UserRoleOptions.Student.ToString())
        {
            events = events.Where(e => subscribedEventIds.Contains(e.Id));
        }

        var facultyNames = await GetFacultyNamesAsync(events);

        return events.Select(e => e.ToAcademicEventResponse(
            e.FacultyId.HasValue && facultyNames.TryGetValue(e.FacultyId.Value, out var name) ? name : null,
            subscribedEventIds.Contains(e.Id)));
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

        if (user.Role != UserRoleOptions.Admin.ToString())
        {
            if (user.Role == UserRoleOptions.Student.ToString())
            {
                if (academicEvent is FacultyEvent fe && (user.Faculty == null || fe.FacultyId != user.Faculty.Id))
                {
                    throw new KeyNotFoundException("Event not found.");
                }
            }
            else // Manager
            {
                if (academicEvent is UniversityEvent && user.Faculty != null)
                {
                    throw new KeyNotFoundException("Event not found.");
                }
                if (academicEvent is FacultyEvent fe && (user.Faculty == null || fe.FacultyId != user.Faculty.Id))
                {
                    throw new KeyNotFoundException("Event not found.");
                }
            }
        }
    }
}
