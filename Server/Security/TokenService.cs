using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Server.Domain.Entities;
using Server.Security.Interfaces;

namespace Server.Security;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateAccessToken(CustomIdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };

        var accessTokenKey = configuration.GetSection("Authentication:AccessTokenSecurityKey").Value!;

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(accessTokenKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(configuration.GetSection("Authentication:AccessTokenExpireMin").Value!)),
            SigningCredentials = credentials
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }
}
  