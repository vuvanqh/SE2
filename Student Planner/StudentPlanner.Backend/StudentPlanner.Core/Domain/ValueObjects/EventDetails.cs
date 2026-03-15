using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Domain;

public class EventDetails //represents a final validated data that is to be passed as a response so shouldnt be mutable?
{
    [StringLength(50)]
    public string Title { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    [StringLength(70)]
    public string? Location { get; private set; }
    public string? Description { get; private set; }
    private EventDetails() { }
    public EventDetails(
        string title,
        DateTime startTime,
        DateTime endTime,
        string location,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (endTime <= startTime)
            throw new ArgumentException("EndTime must be after StartTime");

        Title = title;
        StartTime = startTime;
        EndTime = endTime;
        Location = location;
        Description = description;
    }
}