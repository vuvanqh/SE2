using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure.IdentityEntities;

public class AppFaculty
{
    public required Guid Id { get; set; }
    public required string FacultyId { get; set; }
    public required string FacultyName { get; set; }
    public required string FacultyCode { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public Faculty ToFaculty()
    {
        return new Faculty
        {
            FacultyCode = FacultyCode,
            FacultyId = FacultyId,
            Id = Id,
            FacultyName = FacultyName,
        };
    }
}
