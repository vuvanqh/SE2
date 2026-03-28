using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentPlanner.Core.Application.Authentication;

public record RefreshTokenResponse
{
    [Required] public string AccessToken { get; set; } = null!;
    [Required] public string RefreshToken { get; set; } = null!;
    [Required] public DateTime ExpirationDate { get; set; }
}

public record RefreshTokenResult
{
    [Required] public string RefreshToken { get; set; } = null!;
    [Required] public DateTime ExpirationDate { get; set; }
}

