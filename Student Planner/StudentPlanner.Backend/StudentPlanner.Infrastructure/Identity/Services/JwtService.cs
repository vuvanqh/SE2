using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudentPlanner.Core.Application;
using StudentPlanner.Core.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()), //issued at
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)); //secret key
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
}
