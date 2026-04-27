namespace StudentPlanner.Core.Application.Events;

public record EventPreviewQuery
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public List<Guid>? FacultyIds { get; init; }
}
