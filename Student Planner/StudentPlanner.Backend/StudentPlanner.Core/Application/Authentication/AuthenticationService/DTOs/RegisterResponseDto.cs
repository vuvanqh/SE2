using System;

namespace StudentPlanner.Core.Application.Authentication;

public record RegisterResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
