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
        return events.Select(e => e.ToAcademicEventResponse());
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

        return e.ToAcademicEventResponse();
    }
}
