using System;

namespace StudentPlanner.Core.Domain;

public class AcademicEventSubscriber
{
    public required Guid AcademicEventId { get; set; }
    public required Guid UserId { get; set; }
}
