using System.ComponentModel.DataAnnotations;
using StudentPlanner.Core;

namespace StudentPlanner.Core.Domain;

public class Event
{
    [Key]
    public required Guid Id { get; set; }
    public required EventDetails EventDetails { get; set; }
}
