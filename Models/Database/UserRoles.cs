namespace NetCoreBootstrap.Models.Database
{
    public class UserRoles
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}