using Microsoft.AspNetCore.Identity;

namespace Server.Domain.Entities;

public class Role : IdentityRole
{
    public string? Description { get; init; }
}