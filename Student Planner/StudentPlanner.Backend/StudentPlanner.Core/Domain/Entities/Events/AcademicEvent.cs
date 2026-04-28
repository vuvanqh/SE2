using System.ComponentModel.DataAnnotations.Schema;
// using StudentPlanner.Core.Domain.Entities; // Reverted

namespace StudentPlanner.Core.Domain;

public abstract class AcademicEvent : Event
{
    public Guid? FacultyId { get; set; }


    public ICollection<AcademicEventSubscriber> Subscribers { get; set; } = new List<AcademicEventSubscriber>();
}

public class FacultyEvent : AcademicEvent
{
    public FacultyEvent() { }
    public FacultyEvent(Guid facultyId)
    {
        FacultyId = facultyId;
    }
}

public class UniversityEvent : AcademicEvent
{
}
