namespace NetCoreBootstrap.Mail
{
    public interface IMailer
    {
        void SendMail(string toAddress, string subject, string body, bool isHtml = true);
    }
}
