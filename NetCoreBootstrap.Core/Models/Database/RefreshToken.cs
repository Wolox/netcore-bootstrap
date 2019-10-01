using System;

namespace NetCoreBootstrap.Core.Models.Database
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime ValidFrom { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
    }
}
