#region Using
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#endregion

namespace NetCoreBootstrap.Models.Database
{
    public class DataBaseContext : IdentityDbContext<User>
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserRoles>().HasKey(c => new { c.Id });
            modelBuilder.Entity<UserRoles>()
                        .HasOne(bc => bc.User)
                        .WithMany(b => b.Roles)
                        .HasForeignKey(d => d.UserId);
            modelBuilder.Entity<UserRoles>()
                        .HasOne(bc => bc.Role)
                        .WithMany(c => c.Users)
                        .HasForeignKey(d => d.RoleId);
        }
    }
}
