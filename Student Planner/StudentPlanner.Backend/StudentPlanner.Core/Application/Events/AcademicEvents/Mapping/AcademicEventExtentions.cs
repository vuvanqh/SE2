using StudentPlanner.Core.Application.AcademicEvents.DTOs;
using StudentPlanner.Core.Domain;
using System.Linq;

namespace StudentPlanner.Core.Application.AcademicEvents.Mapping;

public static class AcademicEventExtentions
{
    public static AcademicEventResponse ToAcademicEventResponse(this AcademicEvent academicEvent, bool isSubscribed = false)
    {
        return new AcademicEventResponse
        {
            Id = academicEvent.Id,
            FacultyId = academicEvent.FacultyId,
            Title = academicEvent.EventDetails.Title,
            Description = academicEvent.EventDetails.Description,
            StartTime = academicEvent.EventDetails.StartTime,
            EndTime = academicEvent.EventDetails.EndTime,
            Location = academicEvent.EventDetails.Location,
            IsSubscribed = isSubscribed
        };
    }
}
