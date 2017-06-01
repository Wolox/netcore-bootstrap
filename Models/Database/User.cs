using System.ComponentModel.DataAnnotations.Schema;

namespace MyMVCProject.Models.Database
{
    [Table("user")]
    public class User : BaseEntity
    {
        [Column("firstname")]
        public string FirstName { get; set; }


        [Column("lastname")]
        public string LastName { get; set; }


        [Column("email")]
        public string Email { get; set; }
    }
}
