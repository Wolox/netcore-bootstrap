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
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(c => new { c.UserId, c.RoleId });
            modelBuilder.Entity<User>()
                        .HasMany(e => e.Roles)
                        .WithOne()
                        .HasForeignKey(e => e.UserId)
                        .IsRequired()
                        .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Role>()
                        .HasMany(e => e.Users)
                        .WithOne()
                        .HasForeignKey(e => e.RoleId)
                        .IsRequired()
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
