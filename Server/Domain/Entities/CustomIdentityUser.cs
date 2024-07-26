using Microsoft.AspNetCore.Identity;

namespace Server.Domain.Entities;

public class CustomIdentityUser : IdentityUser
{
    public string? FirstName { get; init; }
    
    public string? LastName { get; init; }
    
    public int? Age { get; init; }
}