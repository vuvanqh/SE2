using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.Java;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentPlanner.Infrastructure.Identity;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        DateTime expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:Expiration_Minutes"]));
        Claim[] claims = new Claim[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //token subject identifier
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //token identifier
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),ClaimValueTypes.Integer64), //issued at
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!)); //secret key
        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken tokenGenerator = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(tokenGenerator);
    }
    public double GetMaxSessionLifetimeDays() => Convert.ToInt32(_config["RefreshToken:max_session_lifetime_days"]);
    public RefreshTokenResult GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);

        DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["RefreshToken:expiration_minutes"]));
        return new RefreshTokenResult()
        {
            RefreshToken = Convert.ToBase64String(bytes),
            ExpirationDate = expiration
        };
    }
}
