using StudentPlanner.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Entities;

public class User
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
    public DateTime RefreshTokenIssuedAt { get; set; }
    public string? UsosToken { get; set; }

    public Faculty? Faculty { get; set; }
}
