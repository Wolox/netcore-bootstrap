namespace NetCoreBootstrap.Core.Models.VOs
{
    public class UserVO
    {
        public UserVO() { }

        public UserVO(string email, string token)
        {
            this.Email = email;
            this.Token = token;
        }

        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
