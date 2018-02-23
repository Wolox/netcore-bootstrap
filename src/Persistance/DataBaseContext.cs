#region Using
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Database;

#endregion

namespace NetCoreBootstrap.Persistance
{
    public class DataBaseContext : IdentityDbContext<User>
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }
        public virtual DbSet<ExampleModel> ExampleModels { get; set; } // TODO DELETE THIS LINE
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
