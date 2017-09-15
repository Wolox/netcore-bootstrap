#region Using
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

            modelBuilder.Entity<User>()
                        .HasMany(e => e.Roles)
                        .WithOne()
                        .HasForeignKey(e => e.UserId)
                        .IsRequired()
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
