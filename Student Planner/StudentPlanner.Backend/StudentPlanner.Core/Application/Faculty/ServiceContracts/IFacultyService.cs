using StudentPlanner.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.ServiceContracts;

public interface IFacultyService
{
    Task<List<FacultyResponse>> GetAllFaculties();
    Task<FacultyResponse?> GetFacultyByIdAsync(Guid id);
}
