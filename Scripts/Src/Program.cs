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
            foreach (var Dir in Directory.GetCurrentDirectory().Split('/'))
            {
                BootstrapRootDir += Dir + "/";
                if (Dir == AppName) break;
            }
            Console.WriteLine("Replacing " + BootstrapName + " to " + AppName + " in " + BootstrapRootDir);
            string Contents = "";
            foreach (string file in Directory.EnumerateFiles(BootstrapRootDir, "*", SearchOption.AllDirectories))
            {
                if (file.Contains("bootstrap-script"))
                    continue;
                Contents = File.ReadAllText(file);
                if (Contents.Contains(BootstrapName))
                {
                    Contents = Contents.Replace(BootstrapName, AppName);
                    if (file == "README.md")
                    {
                        Contents = Contents.Replace("### [Kickoff] Application Setup","");
                        Contents = Contents.Replace("After cloning the bootstrap, run `chmod +x ./Scripts/script.sh`","");
                        Contents = Contents.Replace("Then `./Scripts/script.sh AppName` where `AppName` is your application name.","");
                        Contents = Contents.Replace("Your app is ready. Happy coding!","");
                    }
                    File.WriteAllText(file, Contents);
                }
            }   
            Console.WriteLine("Renaming .csproj ...");
            File.Move(BootstrapRootDir + BootstrapName + ".csproj", BootstrapRootDir + AppName + ".csproj");
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move(BootstrapRootDir + "Scripts/appsettings.Development.json", BootstrapRootDir + "appsettings.Development.json");
            Console.WriteLine(AppName + " is ready! Happy coding!");
            return 1;
        }
    }    
}
