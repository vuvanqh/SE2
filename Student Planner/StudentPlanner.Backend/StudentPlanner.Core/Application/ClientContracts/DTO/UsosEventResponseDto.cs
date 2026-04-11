using System.Text.Json.Serialization;
namespace StudentPlanner.Core.Application.ClientContracts.DTO;
public record UsosEventResponseDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("start_time")]
    public string? StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }

    [JsonPropertyName("course_id")]
    public string? CourseId { get; set; }

    [JsonPropertyName("class_type")]
    public string? ClassType { get; set; }

    [JsonPropertyName("group_number")]
    public string? GroupNumber { get; set; }

    [JsonPropertyName("building_id")]
    public string? BuildingId { get; set; }

    [JsonPropertyName("building_name")]
    public string? BuildingName { get; set; }

    [JsonPropertyName("room_number")]
    public string? RoomNumber { get; set; }

    [JsonPropertyName("room_id")]
    public string? RoomId { get; set; }
}