using Server.Domain.Entities;

namespace Server.Security.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(CustomIdentityUser user);
}