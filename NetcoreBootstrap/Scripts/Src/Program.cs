﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BootstrapScript
{
    class Program
    {
        static int RemoveThisMain(string[] args)
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
                if (dir == PascalToKebabCase(appName)) break;
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
                        contents = contents.Replace($@"### [Kickoff] Application Setup{endOfLine}{endOfLine}","")
                                            .Replace($@"After cloning the bootstrap, follow the [kickoff guide](https://github.com/Wolox/tech-guides/blob/master/net-core/docs/kickoff/README.md#kickoff).{endOfLine}","")
                                            .Replace($@"And happy coding!{endOfLine}","");
                    }
                    if(deleteAuth == DeleteAuthenticationParamValue())
                    {
                        if(file.Contains("/Startup.cs"))
                        {
                            Console.WriteLine("Updating Startup.cs ...");
                            // We use spaces to delete indentation
                            contents = contents.Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", "")
                                                .Replace($@"using Microsoft.AspNetCore.Identity;{endOfLine}", "")
                                                .Replace($@"using Microsoft.AspNetCore.Authentication.Google;{endOfLine}", "")
                                                .Replace($@"app.UseAuthentication();{endOfLine}            ", "");
                            startIndex = contents.IndexOf("// Begin for Identity");
                            endIndex = contents.IndexOf("// Final for Identity", startIndex + 1);
                            contents = contents.Remove(startIndex, (endIndex - startIndex));
                            contents = contents.Replace($"// Final for Identity;{endOfLine}", "");
                        }
                        else if(file.Contains("/Models/Database/DataBaseContext.cs"))
                        {
                            Console.WriteLine("Updating DataBaseContext.cs ...");
                            contents = contents.Replace("IdentityDbContext<User>", "DbContext")
                                                .Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", "")
                                                .Replace($@"using Microsoft.AspNetCore.Identity;{endOfLine}", "");
                        }
                        else if(file.Contains("/Views/Shared/_Layout.cshtml"))
                        {
                            // We use spaces to delete indentation
                            Console.WriteLine("Updating _Layout.cshtml ...");
                            contents = contents.Replace($"    @await Html.PartialAsync(\"_UserManagementPartial\"){endOfLine}                ", "")
                                                .Replace($"    @await Html.PartialAsync(\"_LoginPartial\"){endOfLine}            ", "");
                        }
                    }
                    File.WriteAllText(file, contents);
                }
                else if(file.Contains($@"{bootstrapRootDir}{bootstrapName}.csproj") && deleteAuth == DeleteAuthenticationParamValue())
                {
                    Console.WriteLine("Updating csproj file ...");
                    // We use spaces to delete indentation
                    contents = contents.Replace($"<PackageReference Include=\"Microsoft.AspNetCore.Identity.EntityFrameworkCore\" Version=\"1.1.1\"/>{endOfLine}    ", "")
                                        .Replace($"<PackageReference Include=\"Microsoft.AspNetCore.Authentication.Google\" Version=\"1.1.2\"/>{endOfLine}    ", "");
                    File.WriteAllText(file, contents);
                }
            }   
            Console.WriteLine("Renaming .csproj ...");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/{bootstrapName}.csproj", $@"{bootstrapRootDir}/NetcoreBootstrap/{appName}.csproj");
            if(deleteAuth == DeleteAuthenticationParamValue()) 
            {
                Console.WriteLine("Preparing to delete authentication files ...");
                MoveAuthFilesToScriptFolder(bootstrapRootDir);
            } 
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/appsettings.Development.json", $@"{bootstrapRootDir}/NetcoreBootstrap/appsettings.Development.json");
            Console.WriteLine(appName + " is ready! Happy coding!");
            return 1;
        }

        private static void MoveAuthFilesToScriptFolder(string bootstrapRootDir)
        {
            Directory.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Migrations", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/Migrations");
            Directory.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Views/Account", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ViewsAccount");
            Directory.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Views/UserManagement", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ViewsUserManagement");
            Directory.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Models/Views", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ModelsViews");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Controllers/AccountController.cs", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ControllersAccountController.cs");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Controllers/UserManagementController.cs", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ControllersUserManagementController.cs");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Models/Database/User.cs", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/ModelsDatabaseUser.cs");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Repositories/UserRepository.cs", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/UserRepository.cs");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Views/Shared/_LoginPartial.cshtml", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/_LoginPartial.cshtml");
            File.Move($@"{bootstrapRootDir}/NetcoreBootstrap/Views/Shared/_UserManagementPartial.cshtml", $@"{bootstrapRootDir}/NetcoreBootstrap/Scripts/_UserManagementPartial.cshtml");
        }

        private static string DeleteAuthenticationParamValue()
        {
            return "delete-auth";
        }
        
        public static string PascalToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
                
            return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }
    }
}
