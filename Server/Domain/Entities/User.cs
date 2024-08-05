using Microsoft.AspNetCore.Identity;

namespace Server.Domain.Entities;

public class User : IdentityUser
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public int? Age { get; init; }
    
    public DateTime CreatedAt = DateTime.UtcNow;
    
    /*** Relations ***/
    public List<RefreshToken> RefreshTokens { get; init; } = [];
}