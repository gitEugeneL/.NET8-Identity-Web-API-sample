using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<CustomIdentityUser>(options)
{
    public required DbSet<CustomIdentityUser> CustomUsers { get; init; } 
}
