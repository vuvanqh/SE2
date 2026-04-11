using System;
using System.Collections.Generic;

namespace StudentPlanner.Core.Domain;

public class AcademicEvent : Event
{
    public required Guid FacultyId { get; set; }
    public ICollection<AcademicEventSubscriber> Subscribers { get; set; } = new List<AcademicEventSubscriber>();
}
