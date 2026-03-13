using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace StudentPlanner.Infrastructure.IdentityEntities;

public class ApplicationUser: IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
    public DateTime RefreshTokenIssuedAt { get; set; }
}
