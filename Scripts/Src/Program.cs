﻿using System;
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
            foreach (string file in Directory.  EnumerateFiles(BootstrapRootDir, "*", SearchOption.AllDirectories))
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
            Console.WriteLine("Renaming .csproj ...");
            File.Move(BootstrapRootDir + BootstrapName + ".csproj", BootstrapRootDir + AppName + ".csproj");
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move(BootstrapRootDir +  "Scripts/appsettings.Development.json", BootstrapRootDir +  "appSettings.Development.json");
            return 1;
        }
    }    
}
