using System;
using System.IO;
using System.Linq;

namespace BootstrapScript
{
    class Program
    {
        static int Main(string[] args)
        {
            string appName = args[0];
            Console.WriteLine("Setting up bootstrap for " + appName);
            string bootstrapRootDir = "";
            string bootstrapName = "NetCoreBootstrap";
            foreach (var dir in Directory.GetCurrentDirectory().Split('/'))
            {
                bootstrapRootDir += dir + "/";
                if (dir == appName) break;
            }
            Console.WriteLine("Replacing " + bootstrapName + " to " + appName + " in " + bootstrapRootDir);
            string contents = "";
            var files = from string file in Directory.EnumerateFiles(bootstrapRootDir, "*", SearchOption.AllDirectories) where !file.Contains("bootstrap-script") select file;
            foreach (string file in files)
            {
                contents = File.ReadAllText(file);
                if (contents.Contains(bootstrapName))
                {
                    contents = contents.Replace(bootstrapName, appName);
                    if (file.Contains("/README.md"))
                    {
                        contents = contents.Replace("### [Kickoff] Application Setup\n","")
                                            .Replace("After cloning the bootstrap, run `chmod +x ./Scripts/script.sh`\n","")
                                            .Replace("Then `./Scripts/script.sh AppName` where `AppName` is your application name.\n","")
                                            .Replace("Your app is ready. Happy coding!\n","");
                    }
                    File.WriteAllText(file, contents);
                }
            }   
            Console.WriteLine("Renaming .csproj ...");
            File.Move($@"{bootstrapRootDir}{bootstrapName}.csproj", $@"{bootstrapRootDir}{appName}.csproj");
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move($@"{bootstrapRootDir}Scripts/appsettings.Development.json", $@"{bootstrapRootDir}appsettings.Development.json");
            Console.WriteLine(appName + " is ready! Happy coding!");
            return 1;
        }
    }    
}
