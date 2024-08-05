using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
    public override required DbSet<User> Users { get; set; }
    public required DbSet<RefreshToken> RefreshTokens { get; set; } 
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId);
    }
}
