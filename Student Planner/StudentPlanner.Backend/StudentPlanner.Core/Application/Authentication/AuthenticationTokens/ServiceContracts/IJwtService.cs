using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Entities;

namespace StudentPlanner.Core.Application;

public interface IJwtService
{
    string CreateToken(User user);
    RefreshTokenResult GenerateRefreshToken();
    double GetMaxSessionLifetimeDays();
}
