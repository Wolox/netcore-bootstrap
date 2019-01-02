using System.ComponentModel.DataAnnotations;

namespace NetCoreBootstrap.Models.VOs
{
    public class UserVO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string NewPassword { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        public string UserId { get; set; }
        public bool IsFacebook { get; set; }
        public bool IsGoogle { get; set; }
    }
}
