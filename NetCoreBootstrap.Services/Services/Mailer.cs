using System;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NetCoreBootstrap.Services.Intefaces;

namespace NetCoreBootstrap.Services
{
    public class Mailer : IMailer
    {
        private string _host, _username, _password, _name, _email;
        private int _hostPort;
        private readonly SmtpClient _client;
        private readonly MailboxAddress _fromAddress;

        public Mailer(IConfiguration configuration)
        {
            _host = configuration["Mailer:Host"];
            _hostPort = Convert.ToInt32(configuration["Mailer:Port"]);
            _username = configuration["Mailer:Username"];
            _password = configuration["Mailer:Password"];
            _name = configuration["Mailer:Name"];
            _email = configuration["Mailer:Email"];
            _fromAddress = new MailboxAddress(_name, _email);
            _client = new SmtpClient();
            _client.Connect(_host, _hostPort, false);
            _client.Authenticate(_username, _password);
        }

        public string Host => _host;
        public int HostPort => _hostPort;
        public string Username => _username;
        public string Password => _password;
        public string Name => _name;
        public string Email => _email;
        public MailboxAddress FromMailbox => _fromAddress;

        public void SendMail(string toAddress, string subject, string body, bool isHtml = true)
        {
            var message = new MimeMessage();
            message.From.Add(FromMailbox);
            message.To.Add(new MailboxAddress(toAddress));
            message.Subject = subject;
            var builder = new BodyBuilder();
            builder.TextBody = body;
            message.Body = builder.ToMessageBody();
            using (var client = new SmtpClient())
            {
                client.Connect(Host, HostPort, false);
                client.Authenticate(Username, Password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
