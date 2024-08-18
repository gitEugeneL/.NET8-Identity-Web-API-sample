using Server.Domain.Entities;

namespace Server.Services.Interfaces;

public interface ISecurityService
{
    string GenerateAccessToken(User user, IList<string> roles);
    RefreshToken GenerateRefreshToken(User user);
    void UpdateRefreshTokenCount(User user);
    bool ValidateRefreshToken(RefreshToken refreshToken);
}