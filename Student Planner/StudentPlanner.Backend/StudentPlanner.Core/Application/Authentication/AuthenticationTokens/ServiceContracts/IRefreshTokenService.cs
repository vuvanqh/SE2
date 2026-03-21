using StudentPlanner.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Authentication;

public interface IRefreshTokenService
{
    Task<(User, RefreshTokenResult)> RotateTokenAsync(string currentToken);
    Task<RefreshTokenResult> IssueOnLogin(User user);
    string HashToken(string token);
}
