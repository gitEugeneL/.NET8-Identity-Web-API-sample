using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Domain.Entities;

namespace Server.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User, Role, string>(options)
{
    public override required DbSet<User> Users { get; set; }
    public override required DbSet<Role> Roles { get; set; }
    public required DbSet<RefreshToken> RefreshTokens { get; init; } 
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<User>()
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId);
        
        builder.Entity<Role>()
            .HasData(
                new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Helpers.Roles.Admin,
                    NormalizedName = Helpers.Roles.Admin.ToUpper(),
                    Description = "The admin role"
                },
                new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Helpers.Roles.User,
                    NormalizedName = Helpers.Roles.User.ToUpper(),
                    Description = "The user role"
                }
            );
    }
}
