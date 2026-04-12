using StudentPlanner.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain.RepositoryContracts;

public interface IFacultyRepository
{
    Task<Faculty?> GetFacultyByUsosIdAsync(string facultyId);
    Task<Faculty?> GetFacultyByFacultyCodeAsync(string facultycode);
}
