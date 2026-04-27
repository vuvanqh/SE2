namespace StudentPlanner.Core.Application.Events;

public record EventPreviewQuery
{
    public DateTime? From { get; init; }
    public int? Days { get; init; } = 31;
    public List<Guid>? FacultyIds { get; init; }
}
