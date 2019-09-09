using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using NetCoreBootstrap.Services.Intefaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreBootstrap.Services
{
    public class Mailer : IMailer
    {
        private string _host, _username, _password, _name, _email;
        private int _hostPort;

        public Mailer(IConfiguration configuration)
        {
            _host = configuration["Mailer:Host"];
            _hostPort = Convert.ToInt32(configuration["Mailer:Port"]);
            _username = configuration["Mailer:Username"];
            _password = configuration["Mailer:Password"];
            _name = configuration["Mailer:Name"];
            _email = configuration["Mailer:Email"];
        }

        public string Host => _host;
        public int HostPort => _hostPort;
        public string Username => _username;
        public string Password => _password;
        public string Name => _name;
        public string Email => _email;

        public void SendMail(string toAddress, string subject, string body, bool isHtml = true)
        {
            SmtpClient client = new SmtpClient(Host, HostPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Username, Password),
                EnableSsl = true,
            };
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(Email, Name),
                Body = body,
                Subject = subject,
                IsBodyHtml = isHtml,
            };
            mailMessage.To.Add(toAddress);
            client.Send(mailMessage);
        }
    }
}
