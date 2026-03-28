using StudentPlanner.Core.Domain.RepositoryContracts;
using StudentPlanner.Core.Entities;
using System.Security.Cryptography;
using System.Text;

namespace StudentPlanner.Core.Application.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IJwtService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    public RefreshTokenService(IJwtService tokenService, IIdentityService identityService, IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _identityService = identityService;
        _userRepository = userRepository;
    }
    public string HashToken(string token)
    {
        using var hash = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(hash.ComputeHash(bytes));
    }

    public async Task<(User, RefreshTokenResult)> RotateTokenAsync(string currentToken)
    {
        User? user = await _userRepository.GetUserByRefreshToken(HashToken(currentToken));

        if (user == null)
            throw new ApplicationException("Invalid Token");

        if (user.RefreshTokenExpirationDate < DateTime.UtcNow)
            throw new ApplicationException("Refresh token expired");

        if (user.RefreshTokenIssuedAt.AddDays(_tokenService.GetMaxSessionLifetimeDays()) < DateTime.UtcNow)
            throw new ApplicationException("Session Expired");

        return (user, await IssueAndPersistChanges(user));
    }
    public async Task<RefreshTokenResult> IssueOnLogin(User user)
    {
        user.RefreshTokenIssuedAt = DateTime.UtcNow;

        return await IssueAndPersistChanges(user);
    }


    private async Task<RefreshTokenResult> IssueAndPersistChanges(User user)
    {
        RefreshTokenResult result = _tokenService.GenerateRefreshToken();
        var tokenHash = HashToken(result.RefreshToken);
        var expiratinoDate = result.ExpirationDate;
        await _identityService.UpdateToken(user.Email, tokenHash, expiratinoDate);

        return result;
    }
}
