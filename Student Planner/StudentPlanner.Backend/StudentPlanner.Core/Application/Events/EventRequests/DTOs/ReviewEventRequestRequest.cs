using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Core.Application.EventRequests;

public class ReviewEventRequestRequest
{
    public string? ReviewComment { get; set; }
}