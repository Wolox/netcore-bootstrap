using System.ComponentModel.DataAnnotations;

namespace NetCoreBootstrap.Core.Models.VOs
{
    public class UserSignUpVO
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string UserName { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        [EmailAddress]
        public string Email { get; set; }
        public string Token { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string Password { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 5)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public bool IsFacebook { get; set; }
        public bool IsGoogle { get; set; }
    }
}
