using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dollars.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // call this first

        builder.Entity<AppUser>(e =>
        {
            e.Property(p => p.EnableNotifications).HasDefaultValue(true);
        });

        builder.HasDefaultSchema("identity");
    }
}


/// <summary>
/// Custom user class for idenity users
/// Primary key is an int
/// Custom properties to store for each user
/// </summary>
public class AppUser : IdentityUser<int>
{
    public bool EnableNotifications { get; set; }
}

public class AppRole : IdentityRole<int>
{

}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}