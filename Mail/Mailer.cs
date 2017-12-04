using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetCoreBootstrap.Mail
{
    public class Mailer
    {
        private string _host, _username, _password, _name, _email;
        private int _hostPort;
        private readonly string _jsonFilePath;
        private readonly char _jsonSplitter = ':';
        
        public Mailer()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if(environment == "Production") _jsonFilePath = "appsettings.json";
            else _jsonFilePath = $"appsettings.{environment}.json";
            SetAccountConfiguration();
        }

        public void Send(string toAddress, string subject, string body)
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
                        _host = TryGetValue(resource, "Mailer:Host").ToString();
                        _hostPort = Convert.ToInt32(TryGetValue(resource, "Mailer:Port").ToString());
                        _username = TryGetValue(resource, "Mailer:Username").ToString();
                        _password = TryGetValue(resource, "Mailer:Password").ToString();
                        _name = TryGetValue(resource, "Mailer:Name").ToString();
                        _email = TryGetValue(resource, "Mailer:Email").ToString();
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
            get { return _host; }
        }

        public int HostPort
        {
            get { return _hostPort; }
        }

        public string Username 
        { 
            get { return _username; }
        }

        public string Password 
        { 
            get { return _password; }
        }

        public string Name 
        {
            get { return _name; }
        }

        public string Email 
        { 
            get { return _email; } 
        }
    }
}
