using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Domain;

public class EventDetails: IEquatable<EventDetails> //represents a final validated data that is to be passed as a response so shouldnt be mutable?
{
    [StringLength(50)]
    public string Title { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [StringLength(70)]
    public string? Location { get; set; }
    public string? Description { get; set; }

    public bool Equals(EventDetails? other)
    {
        return other!= null &&
            Title == other.Title &&
            StartTime == other.StartTime &&
            EndTime == other.EndTime &&
            Location == other.Location &&
            Description == other.Description;
    }
}