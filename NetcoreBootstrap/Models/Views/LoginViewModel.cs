using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace NetCoreBootstrap.Models.Views
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }
        
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public ICollection<AuthenticationScheme> LoginProviders { get; set; }
    }
}
