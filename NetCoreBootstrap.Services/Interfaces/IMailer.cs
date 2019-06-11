namespace NetCoreBootstrap.Services.Intefaces
{
    public interface IMailer
    {
        void SendMail(string toAddress, string subject, string body, bool isHtml = true);
    }
}
