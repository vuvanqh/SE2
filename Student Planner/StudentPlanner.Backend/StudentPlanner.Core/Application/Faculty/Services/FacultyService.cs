using StudentPlanner.Core.Application.DTOs;
using StudentPlanner.Core.Application.ServiceContracts;
using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Services;

public class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _facilityRepository;
    public FacultyService(IFacultyRepository facilityRepository)
    {
        _facilityRepository = facilityRepository;
    }

    public async Task<FacultyResponse?> GetFacultyByIdAsync(Guid id)
    {
        return (await _facilityRepository.GetFacultyByIdAsync(id))?.ToFacultyResponse();
    }

    public async Task<List<FacultyResponse>> GetAllFaculties()
    {
        return (await _facilityRepository.GetAllFacultiesAsync()).Select(x => x.ToFacultyResponse()).ToList();
    }
}
