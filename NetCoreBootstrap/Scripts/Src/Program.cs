﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BootstrapScript
{
    public class Program
    {
        public static int RemoveThisMain(string[] args)
        {
            string appName = args[0];
            string deleteAuth = string.Empty;
            if (args.Length > 1) deleteAuth = args[1];
            if (!string.IsNullOrEmpty(deleteAuth) && deleteAuth != DeleteAuthenticationParamValue())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($@"Params not found: {deleteAuth}");
                Console.ResetColor();
            }
            Console.WriteLine("Setting up bootstrap for " + appName);
            string bootstrapRootDir = string.Empty;
            string bootstrapName = "NetCoreBootstrap";
            foreach (var dir in Directory.GetCurrentDirectory().Split('/'))
            {
                bootstrapRootDir += dir + "/";
                if (dir == PascalToKebabCase(appName)) break;
            }
            Console.WriteLine("Replacing " + bootstrapName + " to " + appName + " in " + bootstrapRootDir);
            string contents = string.Empty;
            var files = from string file in Directory.EnumerateFiles(bootstrapRootDir, "*", SearchOption.AllDirectories) where !(file.Contains("bootstrap-script") || file.Contains("script.sh")) select file;
            string endOfLine = string.Empty;
            int startIndex, endIndex;
            foreach (string file in files)
            {
                contents = File.ReadAllText(file);
                if (contents.Contains("\r\n")) endOfLine = "\r\n";
                else if (contents.Contains("\n")) endOfLine = "\n";
                if (contents.Contains(bootstrapName))
                {
                    contents = contents.Replace(bootstrapName, appName);
                    if (file.Contains("/README.md"))
                    {
                        contents = contents.Replace($@"### [Kickoff] Application Setup{endOfLine}{endOfLine}", string.Empty)
                                            .Replace($@"After cloning the bootstrap, follow the [kickoff guide](https://github.com/Wolox/tech-guides/blob/master/net-core/docs/kickoff/README.md#kickoff).{endOfLine}", string.Empty)
                                            .Replace($@"And happy coding!{endOfLine}", string.Empty);
                    }
                    if (deleteAuth == DeleteAuthenticationParamValue())
                    {
                        if (file.Contains("/Startup.cs"))
                        {
                            Console.WriteLine("Updating Startup.cs ...");
                            // We use spaces to delete indentation
                            contents = contents.Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", string.Empty)
                                                .Replace($@"using Microsoft.AspNetCore.Identity;{endOfLine}", string.Empty)
                                                .Replace($@"using Microsoft.AspNetCore.Authentication.Google;{endOfLine}", string.Empty)
                                                .Replace($@"app.UseAuthentication();{endOfLine}            ", string.Empty);
                            startIndex = contents.IndexOf("// Begin for Identity");
                            endIndex = contents.IndexOf("// Final for Identity", startIndex + 1);
                            contents = contents.Remove(startIndex, endIndex - startIndex);
                            contents = contents.Replace($"// Final for Identity;{endOfLine}", string.Empty);
                        }
                        else if (file.Contains("/Models/Database/DataBaseContext.cs"))
                        {
                            Console.WriteLine("Updating DataBaseContext.cs ...");
                            contents = contents.Replace("IdentityDbContext<User>", "DbContext")
                                                .Replace($@"using Microsoft.AspNetCore.Identity.EntityFrameworkCore;{endOfLine}", string.Empty)
                                                .Replace($@"using Microsoft.AspNetCore.Identity;{endOfLine}", string.Empty);
                        }
                        else if (file.Contains("/Views/Shared/_Layout.cshtml"))
                        {
                            // We use spaces to delete indentation
                            Console.WriteLine("Updating _Layout.cshtml ...");
                            contents = contents.Replace($"    @await Html.PartialAsync(\"_UserManagementPartial\"){endOfLine}                ", string.Empty)
                                                .Replace($"    @await Html.PartialAsync(\"_LoginPartial\"){endOfLine}            ", string.Empty);
                        }
                    }
                    File.WriteAllText(file, contents);
                }
                else if (file.Contains($@"{bootstrapRootDir}{bootstrapName}.csproj") && deleteAuth == DeleteAuthenticationParamValue())
                {
                    Console.WriteLine("Updating csproj file ...");
                    // We use spaces to delete indentation
                    contents = contents.Replace($"<PackageReference Include=\"Microsoft.AspNetCore.Identity.EntityFrameworkCore\" Version=\"1.1.1\"/>{endOfLine}    ", string.Empty)
                                        .Replace($"<PackageReference Include=\"Microsoft.AspNetCore.Authentication.Google\" Version=\"1.1.2\"/>{endOfLine}    ", string.Empty);
                    File.WriteAllText(file, contents);
                }
            }
            Console.WriteLine("Renaming .csproj ...");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/{bootstrapName}.csproj", $@"{bootstrapRootDir}/NetCoreBootstrap/{appName}.csproj");
            Console.WriteLine("Renaming Test .csproj ...");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap.Tests/{bootstrapName}.Tests.csproj", $@"{bootstrapRootDir}/NetCoreBootstrap.Tests/{appName}.Tests.csproj");
            Console.WriteLine("Renaming sln ...");
            File.Move($@"{bootstrapRootDir}/netcore-bootstrap.sln", $@"{bootstrapRootDir}/{PascalToKebabCase(appName)}.sln");

            if (deleteAuth == DeleteAuthenticationParamValue())
            {
                Console.WriteLine("Preparing to delete authentication files ...");
                MoveAuthFilesToScriptFolder(bootstrapRootDir);
            }
            Console.WriteLine("Moving appsettings.Development.json ...");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/appsettings.Development.json", $@"{bootstrapRootDir}/NetCoreBootstrap/appsettings.Development.json");
            Console.WriteLine(appName + " is ready! Happy coding!");
            return 1;
        }

        private static void MoveAuthFilesToScriptFolder(string bootstrapRootDir)
        {
            Directory.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Migrations", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/Migrations");
            Directory.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Views/Account", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ViewsAccount");
            Directory.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Views/UserManagement", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ViewsUserManagement");
            Directory.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Models/Views", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ModelsViews");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Controllers/AccountController.cs", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ControllersAccountController.cs");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Controllers/UserManagementController.cs", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ControllersUserManagementController.cs");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Models/Database/User.cs", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/ModelsDatabaseUser.cs");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Repositories/UserRepository.cs", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/UserRepository.cs");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Views/Shared/_LoginPartial.cshtml", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/_LoginPartial.cshtml");
            File.Move($@"{bootstrapRootDir}/NetCoreBootstrap/Views/Shared/_UserManagementPartial.cshtml", $@"{bootstrapRootDir}/NetCoreBootstrap/Scripts/_UserManagementPartial.cshtml");
        }

        private static string DeleteAuthenticationParamValue()
        {
            return "delete-auth";
        }

        private static string PascalToKebabCase(string value)
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
