using StudentPlanner.Core.Application.DTOs;
using StudentPlanner.Core.Domain.Entities;

namespace StudentPlanner.Core.Mapping;

internal static class FacultyExtention
{
    public static FacultyResponse ToFacultyResponse(this Faculty faculty)
    {
        return new FacultyResponse()
        {
            FacultyCode = faculty.FacultyCode,
            FacultyName = faculty.FacultyName,
            FacultyId = faculty.Id,
        };
    }
}
