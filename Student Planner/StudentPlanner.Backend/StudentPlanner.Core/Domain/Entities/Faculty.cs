using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Domain.Entities;

public class Faculty
{
    public required Guid Id { get; set; }
    public required string FacultyId { get; set; }
    public required string FacultyName { get; set; }
    public required string FacultyCode { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
