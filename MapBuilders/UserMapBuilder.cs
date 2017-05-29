using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyMVCProject.Models.Database;

namespace MyMVCProject.MapBuilders 
{
    public class UserMapBuilder 
    {
        public UserMapBuilder(EntityTypeBuilder <User> entityBuilder) {
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.Property(t => t.FirstName).IsRequired();
            entityBuilder.Property(t => t.LastName).IsRequired();
            entityBuilder.Property(t => t.Email).IsRequired();
        }
    }
}
