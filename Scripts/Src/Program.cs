using System;
using System.IO;

namespace bootstrap_script
{
    class Program
    {
        static int Main(string[] args)
        {
            string AppName = args[0];
            Console.WriteLine("Setting up bootstrap for " + AppName);
            string BootstrapRootDir = "";
            string BootstrapName = "NetCoreBootstrap";
            string BootstrapDir = "netcore-bootstrap";
            foreach (var Dir in Directory.GetCurrentDirectory().Split('/'))
            {
                BootstrapRootDir += Dir + "/";
                if (Dir == BootstrapDir) break;
            }
            string FullOldBootstrapDir = BootstrapRootDir + BootstrapDir + "/";
            Console.WriteLine("Replacing " + BootstrapName + " to " + AppName + " in " + FullOldBootstrapDir);
            foreach (string file in Directory.EnumerateFiles(FullOldBootstrapDir, "*", SearchOption.AllDirectories))
            {
                if (file.Contains("bootstrap-script"))
                    continue; 
                string Contents = File.ReadAllText(file);
                if (Contents.Contains(BootstrapName))
                    File.WriteAllText(file, Contents.Replace(BootstrapName, AppName));
                if (file == "README.md")
                {
                    Contents.Replace("### [Kickoff] Application Setup","");
                    Contents.Replace("After cloning the bootstrap, run `chmod +x ./netcore-bootstrap/Scripts/script.sh`","");
                    Contents.Replace("Then `./netcore-bootstrap/Scripts/script.sh AppName` where `AppName` is your application name.","");
                    Contents.Replace("Your app is ready. Happy coding!","");
                }
            }   
            BootstrapDir += "/";
            Console.WriteLine("Renaming csproj file");
            File.Move(FullOldBootstrapDir + BootstrapName + ".csproj", FullOldBootstrapDir + AppName + ".csproj");
            File.Move(FullOldBootstrapDir +  "Scripts/appsettings.Development.json", FullOldBootstrapDir +  "appSettings.Development.json");
            Console.WriteLine("Renaming root directory");
            Directory.Delete(FullOldBootstrapDir + ".git");
            Directory.Move(FullOldBootstrapDir, BootstrapRootDir + AppName);
            return 1;
        }
    }    
}
