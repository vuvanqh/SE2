using System;
using StudentPlanner.Core.Domain;

namespace StudentPlanner.Core.Application.EventRequests.Strategies;

public static class EventRequestValidationHelper
{
    public static void ValidateEventDetails(EventDetails details)
    {
        if (string.IsNullOrWhiteSpace(details.Title)) throw new ArgumentException("Title is mandatory.");
        if (string.IsNullOrWhiteSpace(details.Location)) throw new ArgumentException("Location is mandatory.");
        if (details.StartTime == default) throw new ArgumentException("StartTime is mandatory.");
        if (details.EndTime == default) throw new ArgumentException("EndTime is mandatory.");
        if (details.EndTime <= details.StartTime) throw new ArgumentException("EndTime must be after StartTime.");
    }
}
