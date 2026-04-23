using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using StudentPlanner.Core.Domain;
using StudentPlanner.Core.Domain.Entities;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.Infrastructure.IdentityEntities;

public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
    public DateTime RefreshTokenIssuedAt { get; set; }
    public string? UsosToken { get; set; }

    public Guid? FacultyId { get; set; }
    public AppFaculty? Faculty { get; set; }
    public ICollection<PersonalEvent> PersonalEvents { get; set; } = new List<PersonalEvent>();
    public bool NotificationsEnabled { get; set; } = true;

    public User ToUser(string roleName) => new User()
    {
        Id = Id,
        Email = Email!,
        FirstName = FirstName,
        LastName = LastName,
        RefreshTokenHash = RefreshTokenHash,
        RefreshTokenExpirationDate = RefreshTokenExpirationDate,
        RefreshTokenIssuedAt = RefreshTokenIssuedAt,
        UsosToken = UsosToken,
        Role = roleName,
        Faculty = Faculty?.ToFaculty(),
        NotificationsEnabled = NotificationsEnabled
    };
}
