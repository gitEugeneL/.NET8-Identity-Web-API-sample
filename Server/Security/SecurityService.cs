using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Server.Domain.Entities;
using Server.Security.Interfaces;

namespace Server.Security;

public class SecurityService(IConfiguration configuration) : ISecurityService
{
    public string GenerateAccessToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
        };
        claims.AddRange(roles
                .Select(role => new Claim(ClaimTypes.Role, role)));

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
    
    public RefreshToken GenerateRefreshToken(User user)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256)),
            Expires = DateTime.UtcNow.AddDays(
                int.Parse(configuration.GetSection("Authentication:RefreshTokenLifetimeDays").Value!)),
            User = user
        };
    }

    public void UpdateRefreshTokenCount(User user)
    {
        var maxCount = int.Parse(configuration.GetSection("Authentication:RefreshTokenMaxCount").Value!);

        if (user.RefreshTokens.Count < maxCount) 
            return;
        
        var lastRefreshToken = user
            .RefreshTokens
            .OrderBy(rt => rt.Expires)
            .First();
        
        user.RefreshTokens.Remove(lastRefreshToken);
    }
}
  