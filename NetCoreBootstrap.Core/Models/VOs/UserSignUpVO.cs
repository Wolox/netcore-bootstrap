namespace NetCoreBootstrap.Core.Models.VOs
{
    public class UserSignUpVO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool IsFacebook { get; set; }
        public bool IsGoogle { get; set; }
    }
}
