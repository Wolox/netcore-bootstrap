using System.ComponentModel.DataAnnotations;

namespace NetCoreBootstrap.Models.Views
{
    public class UserViewModel
    {
        [Required]
        public string UserName { get; set; }
        
        [Required, EmailAddress, MaxLength(256), Display(Name = "Email Address")]
        public string Email { get; set; }
        
        [Required, MinLength(6), MaxLength(40), DataType(DataType.Password), Display(Name = "Password")]
        public string Password { get; set; }
     
        [Required, MinLength(6), MaxLength(40), DataType(DataType.Password), Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password doesn't match the confirmation password")]
        public string ConfirmPassword { get; set; }
    }
}