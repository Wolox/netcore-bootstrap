using System.ComponentModel.DataAnnotations;

namespace NetCoreBootstrap.Core.Models.VOs
{
    public class UserSignInVO
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string UserName { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
