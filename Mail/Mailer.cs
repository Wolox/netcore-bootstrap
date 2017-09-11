using System;
using System.IO;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreBootstrap.Mail
{
    public class Mailer
    {
        private readonly SmtpClient _smtpClient;
        private string host, username, password, name, email;
        private int hostPort;
        private readonly string _jsonFilePath;
        private readonly char _jsonSplitter = ':';
        
        public Mailer()
        {
            _smtpClient = new SmtpClient();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if(environment == "Production") _jsonFilePath = "appsettings.json";
            else _jsonFilePath = $"appsettings.{environment}.json";
            SetAccountConfiguration();
        }

        public void Send(string toName, string toAddress, string subject, string body, string type = "plain")
        {
            var message = new MimeMessage { Subject = subject, Body = new TextPart(type) { Text = body }};
            message.From.Add(new MailboxAddress(Name, Email));
            message.To.Add(new MailboxAddress(toName, toAddress));
            using(var client = Client)
            {
                try
                {
                    client.Connect(Host, HostPort);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    if(String.IsNullOrEmpty(Username) || String.IsNullOrEmpty(Password)) throw new ArgumentNullException();
                    client.Authenticate(Username, Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
                catch(Exception e)
                {
                    throw new Exception(message: e.Message);
                }
            }
        }

        private void SetAccountConfiguration()
        {
            try
            {
                var resourceFileStream = new FileStream(JsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, 
                                                        FileOptions.Asynchronous | FileOptions.SequentialScan);
                using (resourceFileStream)
                {
                    var resourceReader = new JsonTextReader(new StreamReader(resourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                    using (resourceReader)
                    {
                        var resource = JObject.Load(resourceReader);
                        if(resource == null) throw new ArgumentNullException();
                        host = TryGetValue(resource, "Mailer:Host").ToString();
                        hostPort = Convert.ToInt32(TryGetValue(resource, "Mailer:Port").ToString());
                        username = TryGetValue(resource, "Mailer:Username").ToString();
                        password = TryGetValue(resource, "Mailer:Password").ToString();
                        name = TryGetValue(resource, "Mailer:Name").ToString();
                        email = TryGetValue(resource, "Mailer:Email").ToString();
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception(message: e.Message);
            }
        }

        private JToken TryGetValue(JObject resource, string name)
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

        public string JsonFilePath
        {
            get { return _jsonFilePath; }
        }

        public char JsonSplitter
        {
            get { return _jsonSplitter; }
        }

        public string Host 
        {
            get { return host; }
        }

        public int HostPort
        {
            get { return hostPort; }
        }

        public SmtpClient Client
        {
            get { return _smtpClient; }
        }

        public string Username 
        { 
            get { return username; }
        }

        public string Password 
        { 
            get { return password; }
        }

        public string Name 
        {
            get { return name; }
        }

        public string Email 
        { 
            get { return email; } 
        }
    }
}
