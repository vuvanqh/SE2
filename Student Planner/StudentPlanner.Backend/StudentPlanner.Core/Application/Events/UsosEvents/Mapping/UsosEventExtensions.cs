using StudentPlanner.Core;
using StudentPlanner.Core.Application.ClientContracts.DTO;
using StudentPlanner.Core.Domain;
using System.Globalization;

namespace StudentPlanner.Core.Application.Events.UsosEvents.Mapping;

public static class UsosEventExtensions
{
    public static UsosEvent ToStudentUsosEvent(this UsosEventResponseDto dto, Guid userId)
    {
        var start = DateTime.Parse(dto.StartTime!, CultureInfo.InvariantCulture);
        var end = DateTime.Parse(dto.EndTime!, CultureInfo.InvariantCulture);

        return new UsosEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = dto.CourseId,
            ClassType = dto.ClassType,
            GroupNumber = dto.GroupNumber,
            BuildingId = dto.BuildingId,
            BuildingName = dto.BuildingName,
            RoomNumber = dto.RoomNumber,
            RoomId = dto.RoomId,
            ExternalKey = BuildExternalKey(dto),
            EventDetails = new EventDetails
            {
                Title = dto.Title ?? "USOS event",
                StartTime = start,
                EndTime = end,
                Location = BuildLocation(dto),
                Description = BuildDescription(dto)
            }
        };
    }

    private static string BuildExternalKey(UsosEventResponseDto dto)
    {
        return string.Join("|",
            dto.CourseId ?? string.Empty,
            dto.ClassType ?? string.Empty,
            dto.GroupNumber ?? string.Empty,
            dto.StartTime ?? string.Empty,
            dto.EndTime ?? string.Empty,
            dto.RoomId ?? string.Empty);
    }

    private static string? BuildLocation(UsosEventResponseDto dto)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.BuildingName))
            parts.Add(dto.BuildingName);

        if (!string.IsNullOrWhiteSpace(dto.RoomNumber))
            parts.Add(dto.RoomNumber);

        var result = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }

    private static string? BuildDescription(UsosEventResponseDto dto)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.CourseId))
            parts.Add($"Course: {dto.CourseId}");

        if (!string.IsNullOrWhiteSpace(dto.ClassType))
            parts.Add($"Type: {dto.ClassType}");

        if (!string.IsNullOrWhiteSpace(dto.GroupNumber))
            parts.Add($"Group: {dto.GroupNumber}");

        var result = string.Join(" | ", parts);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}