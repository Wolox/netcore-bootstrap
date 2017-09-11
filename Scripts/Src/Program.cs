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
            string deleteAuth = "";
            if(args.Length > 1) deleteAuth = args[1];
            if(!String.IsNullOrEmpty(deleteAuth) && deleteAuth != DeleteAuthenticationParamValue())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($@"Params not found: {deleteAuth}");
                Console.ResetColor();
            } 
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
            string endOfLine = "";
            int startIndex, endIndex;
            foreach (string file in files)
            {
                contents = File.ReadAllText(file);
                if(contents.Contains("\r\n")) endOfLine = "\r\n";
                else if(contents.Contains("\n")) endOfLine = "\n";
                if (contents.Contains(bootstrapName))
                {
                    contents = contents.Replace(bootstrapName, appName);
                    if (file.Contains("/README.md"))
                    {
                        contents = contents.Replace($@"### [Kickoff] Application Setup{endOfLine}","")
                                            .Replace($@"After cloning the bootstrap, run `chmod +x ./Scripts/script.sh`{endOfLine}","")
                                            .Replace($@"Then `./Scripts/script.sh AppName` where `AppName` is your application name.{endOfLine}","")
                                            .Replace($@"If you don't need authentication run `./Scripts/script.sh AppName delete-auth`{endOfLine}","")
                                            .Replace($@"Your app is ready. Happy coding!{endOfLine}","");
                    }
                    if(deleteAuth == DeleteAuthenticationParamValue())
                    {
                        if(file.Contains("/Startup.cs"))
                        {
                            Console.WriteLine("Updating Startup.cs ...");
                            // We use spaces to delete indentation
                            contents = contents.Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", "")
                                                .Replace($@"using Microsoft.AspNetCore.Identity;{endOfLine}", "")
                                                .Replace($@"app.UseIdentity();{endOfLine}            ", "");
                            startIndex = contents.IndexOf("services.AddIdentity");
                            endIndex = contents.IndexOf("services.", startIndex + 1);
                            contents = contents.Remove(startIndex, (endIndex - startIndex));
                        }
                        else if(file.Contains("/Models/Database/DataBaseContext.cs"))
                        {
                            Console.WriteLine("Updating DataBaseContext.cs ...");
                            contents = contents.Replace("IdentityDbContext<User>", "DbContext")
                                                .Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", "");
                        }
                    }
                    File.WriteAllText(file, contents);
                }
                else if(file.Contains($@"{bootstrapRootDir}{bootstrapName}.csproj"))
                {
                    Console.WriteLine("Updating csproj file ...");
                    // We use spaces to delete indentation
                    File.WriteAllText(file, contents.Replace($"<PackageReference Include=\"Microsoft.AspNetCore.Identity.EntityFrameworkCore\" Version=\"1.1.1\" />{endOfLine}    ", ""));
                }
            }   
            Console.WriteLine("Renaming .csproj ...");
            File.Move($@"{bootstrapRootDir}{bootstrapName}.csproj", $@"{bootstrapRootDir}{appName}.csproj");
            if(deleteAuth == DeleteAuthenticationParamValue()) 
            {
                Console.WriteLine("Preparing to delete authentication files ...");
                MoveAuthFilesToScriptFolder(bootstrapRootDir);
            } 
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move($@"{bootstrapRootDir}Scripts/appsettings.Development.json", $@"{bootstrapRootDir}appsettings.Development.json");
            Console.WriteLine(appName + " is ready! Happy coding!");
            return 1;
        }

        private static void MoveAuthFilesToScriptFolder(string bootstrapRootDir)
        {
            Directory.Move($@"{bootstrapRootDir}Migrations", $@"{bootstrapRootDir}Scripts/Migrations");
            Directory.Move($@"{bootstrapRootDir}Views/Account", $@"{bootstrapRootDir}Scripts/ViewsAccount");
            Directory.Move($@"{bootstrapRootDir}Views/UserManagement", $@"{bootstrapRootDir}Scripts/ViewsUserManagement");
            Directory.Move($@"{bootstrapRootDir}Models/Views", $@"{bootstrapRootDir}Scripts/ModelsViews");
            File.Move($@"{bootstrapRootDir}Controllers/AccountController.cs", $@"{bootstrapRootDir}Scripts/ControllersAccountController.cs");
            File.Move($@"{bootstrapRootDir}Controllers/UserManagementController.cs", $@"{bootstrapRootDir}Scripts/ControllersUserManagementController.cs");
            File.Move($@"{bootstrapRootDir}Models/Database/User.cs", $@"{bootstrapRootDir}Scripts/ModelsDatabaseUser.cs");
            File.Move($@"{bootstrapRootDir}Repositories/UserRepository.cs", $@"{bootstrapRootDir}Scripts/UserRepository.cs");
        }

        private static string DeleteAuthenticationParamValue()
        {
            return "delete-auth";
        }
    }
}
