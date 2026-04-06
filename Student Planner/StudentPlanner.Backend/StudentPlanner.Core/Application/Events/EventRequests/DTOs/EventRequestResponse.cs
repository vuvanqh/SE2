using StudentPlanner.Core.Domain;
using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.EventRequests;

public record EventRequestResponse
{
    [Required] public Guid Id { get; set; }
    [Required] public Guid FacultyId { get; set; }
    [Required] public Guid ManagerId { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    [Required] public RequestType RequestType { get; set; }
    [Required] public RequestStatus Status { get; set; }
    [Required] public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }
    [Required] public Guid EventId { get; set; }
}