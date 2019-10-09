#!/usr/bin/python
# -*- coding: UTF-8 -*-

import os, argparse, shutil

bootstrap_name = "NetCoreBootstrap"
folder_list = ["Api", "Core", "Data", "Services", "Tests"]
parent_folder = ".."
ignored_folders = [".git", "bin", "obj"]
empty_string = ""
end_of_line = "\r\n"

# Renames files and folders
def __rename_folder_files(app_name):
	for folder in folder_list:
		os.chdir(bootstrap_name + "." + folder)
		__rename_current_directory_files(app_name)
		os.chdir(parent_folder)
	__rename_current_directory_files(app_name)

def __rename_current_directory_files(app_name):
	for file in filter(lambda name: bootstrap_name in name, os.listdir(os.curdir)):
		new_name = file.replace(bootstrap_name, app_name)
		os.rename(file, new_name)

# Rename files
def __rename_files_content(app_name):
	files = __get_files()
	for file_name in files:
		file = open(file_name, 'r').read()
		if bootstrap_name in file:
			print("Updating " + file_name)
			file = file.replace(bootstrap_name, app_name)
		new_file = open(file_name, 'w')
		new_file.write(file)

# Delete auth files
def __delete_auth_files():
	os.remove(os.curdir + "/" + bootstrap_name + ".Api/Controllers/AccountController.cs")
	shutil.rmtree(os.curdir + "/" + bootstrap_name + ".Core/Models/VOs")
	os.remove(os.curdir + "/" + bootstrap_name + ".Core/Models/Database/User.cs")
	os.remove(os.curdir + "/" + bootstrap_name + ".Core/Models/Database/RefreshToken.cs")
	shutil.rmtree(os.curdir + "/" + bootstrap_name + ".Data/Migrations")
	os.remove(os.curdir + "/" + bootstrap_name + ".Data/Repositories/UserRepository.cs")
	os.remove(os.curdir + "/" + bootstrap_name + ".Data/Repositories/Interfaces/IUserRepository.cs")
	shutil.rmtree(os.curdir + "/" + bootstrap_name + ".Services/Helpers")

def __update_database_context():
	db_context = "IdentityDbContext<User>"
	line_to_delete = "\n        public DbSet<RefreshToken> RefreshTokens { get; set; }\n"
	usings = "using Microsoft.AspNetCore.Identity;\nusing Microsoft.AspNetCore.Identity.EntityFrameworkCore;"
	file = open(bootstrap_name + ".Data/Repositories/Database/DatabaseContext.cs", "r").read()
	file = file.replace(db_context, "DbContext")
	file = file.replace(line_to_delete, empty_string)
	file = file.replace(usings, empty_string)
	new_file = open(bootstrap_name + ".Data/Repositories/Database/DatabaseContext.cs", "w")
	new_file.write(file)

def __update_startup():
	use_authentication = "            app.UseAuthentication();" + end_of_line
	usings_to_delete = ["using System;", "using System.Text;", "using Microsoft.AspNetCore.Identity;", "using Microsoft.IdentityModel.Tokens;", "using NameApp.Core.Models.Database;"]
	file = open(bootstrap_name + ".Api/Startup.cs", "r").read()
	file = file.replace(use_authentication, empty_string)
	for using in usings_to_delete:
		file = file.replace(using + end_of_line, empty_string)
	start_identity_index = file.find("            services.AddAuthentication()")
	end_identity_index = file.find(".AddDefaultTokenProviders();") + len(".AddDefaultTokenProviders();")
	file = file[:start_identity_index] + file[end_identity_index:]
	new_file = open(bootstrap_name + ".Api/Startup.cs", "w")
	new_file.write(file)

def __update_unit_of_work_interface():
	interface_to_delete = "        IUserRepository UserRepository { get; }\n"
	file = open(bootstrap_name + ".Data/Repositories/Interfaces/IUnitOfWork.cs", "r").read()
	file = file.replace(interface_to_delete, empty_string)
	new_file = open(bootstrap_name + ".Data/Repositories/Interfaces/IUnitOfWork.cs", "w")
	new_file.write(file)

def __update_unit_of_work():
	args_to_delete = ", UserManager<User> userManager, RoleManager<IdentityRole> roleManager"
	repository_to_delete = "            this.UserRepository = new UserRepository(context, userManager, roleManager);\n"
	property_to_delete = "\n        public IUserRepository UserRepository { get; private set; }\n"
	usings_to_delete = ["using Microsoft.AspNetCore.Identity;", "using NameApp.Core.Models.Database;"]
	file = open(bootstrap_name + ".Data/Repositories/Database/UnitOfWork.cs", "r").read()
	file = file.replace(args_to_delete, empty_string)
	file = file.replace(repository_to_delete, empty_string)
	file = file.replace(property_to_delete, empty_string)
	for using in usings_to_delete:
		file = file.replace(using + "\n", empty_string)
	new_file = open(bootstrap_name + ".Data/Repositories/Database/UnitOfWork.cs", "w")
	new_file.write(file)
	

# Helpers
def __folder_must_be_ignored(folder_name):
	for folder in ignored_folders:
		if folder in folder_name:
			return True
	return False

def __get_files():
	files = []
	for root, directories, filenames in os.walk(os.curdir):
		for filename in filenames:
			if not __folder_must_be_ignored(root) or filename == 'kickoff.py':
				files.append(os.path.join(root, filename))
	return files

# Main function
def rename_all_files(app_name, auth):
	if args.auth == "False":
		__delete_auth_files()
		__update_database_context()
		__update_startup()
		__update_unit_of_work_interface()
		__update_unit_of_work()
	__rename_folder_files(app_name)
	__rename_files_content(args.name)

parser = argparse.ArgumentParser()
parser.add_argument('-auth', help='Authentication true/false')
parser.add_argument('-name', help='Application name')
args = parser.parse_args()
rename_all_files(args.name, args.auth)
os.remove("kickoff.py")
shutil.rmtree(".git")
