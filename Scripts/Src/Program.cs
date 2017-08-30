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
            string BootstrapRootDir="";
            string BootstrapName = "NetCoreBootstrap";
            string BootstrapDir = "netcore-bootstrap";
            foreach (var Dir in Directory.GetCurrentDirectory().Split('/'))
            {
                if (Dir == BootstrapDir)
                    break;
                else 
                    BootstrapRootDir += Dir + "/";
            }
            Console.WriteLine("Replacing " + BootstrapName + " to " + AppName);
            foreach (string file in Directory.EnumerateFiles(BootstrapRootDir, "*", SearchOption.AllDirectories))
            {
                if (file.Contains("bootstrap-script"))
                    continue;
                string Contents = File.ReadAllText(file);
                if (Contents.Contains(BootstrapName))
                    File.WriteAllText(file,Contents.Replace(BootstrapName,AppName));
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
            File.Move(BootstrapRootDir + BootstrapDir + BootstrapName + ".csproj", BootstrapRootDir + BootstrapDir + AppName + ".csproj");
            File.Move(Directory.GetCurrentDirectory() + "/appsettings.Development.json", BootstrapRootDir + BootstrapDir +  "appSettings.Development.json");
            Console.WriteLine("Renaming root directory");
            Directory.Delete(BootstrapRootDir + BootstrapDir + ".git");
            Directory.Move(BootstrapRootDir + BootstrapDir,BootstrapRootDir + AppName);
            return 1;
        }
    }

    
}


