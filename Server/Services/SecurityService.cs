using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Server.Domain.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class SecurityService(
    IConfiguration configuration, 
    UserManager<User> userManager) : ISecurityService
{
    private string CreateParam(string email, string clientUri, string token, string type)
    {
        var param = new Dictionary<string, string>
        {
            { type, token },
            { "email", email }
        }; 
        return QueryHelpers.AddQueryString(clientUri, param!);
    }
    
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
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(configuration.GetSection("Authentication:AccessTokenExpireMin").Value!)),
            SigningCredentials = credentials,
            Issuer = configuration.GetSection("Authentication:Issuer").Value,
            Audience = configuration.GetSection("Authentication:Audience").Value
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

    public async Task<string> GenerateEmailConfirmationToken(User user, string clientUri)
    {
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        return CreateParam(user.Email!, clientUri, confirmationToken, "confirmationToken");
    }

    public async Task<string> GeneratePasswordResetToken(User user, string clientUri)
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        return CreateParam(user.Email!, clientUri, resetToken, "resetToken");
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
    
    public bool ValidateRefreshToken(RefreshToken refreshToken) => 
        refreshToken.Expires >= DateTime.UtcNow;
}
  