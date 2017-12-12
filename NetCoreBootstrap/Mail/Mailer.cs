using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreBootstrap.Mail
{
    public static class Mailer
    {
        private static string host, username, password, name, email;
        private static int hostPort;

        public static string Host
        {
            get { return host; }
        }

        public static int HostPort
        {
            get { return hostPort; }
        }

        public static string Username
        {
            get { return username; }
        }

        public static string Password
        {
            get { return password; }
        }

        public static string Name
        {
            get { return name; }
        }

        public static string Email
        {
            get { return email; }
        }

        public static void Send(string toAddress, string subject, string body)
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
            };
            mailMessage.To.Add(toAddress);
            client.Send(mailMessage);
        }

        public static void SetAccountConfiguration(IConfiguration configuration)
        {
            host = configuration["Mailer:Host"];
            hostPort = Convert.ToInt32(configuration["Mailer:Port"]);
            username = configuration["Mailer:Username"];
            password = configuration["Mailer:Password"];
            name = configuration["Mailer:Name"];
            email = configuration["Mailer:Email"];
        }
    }
}
