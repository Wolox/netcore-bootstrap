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
        private static string jsonFilePath;
        private readonly static char _jsonSplitter = ':';

        public static void Send(string toAddress, string subject, string body)
        {
            SmtpClient client = new SmtpClient(Host,HostPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Username, Password),
                EnableSsl = true
            };    
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(Email,Name),
                Body = body,
                Subject = subject
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
            email =configuration["Mailer:Email"];
        }

        private static JToken TryGetValue(JObject resource, string name)
        {
            JToken jTokenValue = null;
            string[] keys = name.Split(JsonSplitter);
            jTokenValue = resource[keys[0]];            
            for(var i = 1; i < keys.Length; i++)
            {
                jTokenValue = jTokenValue[keys[i]];
            }
            return jTokenValue;
        }

        public static string JsonFilePath
        {
            get { return jsonFilePath; }
        }

        public static char JsonSplitter
        {
            get { return _jsonSplitter; }
        }

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
    }
}
