using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.Infrastructure.IdentityEntities;

public class ApplicationUser: IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
    public DateTime RefreshTokenIssuedAt { get; set; }


    public User ToUser() => new User()
    {
        Id = Id,
        Email = Email!,
        FirstName = FirstName,
        LastName = LastName,
        RefreshTokenHash = RefreshTokenHash,
        RefreshTokenExpirationDate = RefreshTokenExpirationDate,
        RefreshTokenIssuedAt = RefreshTokenIssuedAt
    };
}
