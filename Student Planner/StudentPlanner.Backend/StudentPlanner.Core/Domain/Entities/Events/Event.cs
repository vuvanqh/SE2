using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Domain;

public class Event
{
    [Key]
    public required Guid Id { get; set; }
    public required EventDetails EventDetails { get; set; }
}
