#region Using

using Microsoft.EntityFrameworkCore;

#endregion

namespace MyMVCProject.Models.Database
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }
    }
}
