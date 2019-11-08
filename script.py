#!/usr/bin/python
# -*- coding: UTF-8 -*-

import os
import argparse
import shutil
import contextlib
import io
import sys

bootstrap_name = "NetCoreBootstrap"
folder_list = ["Api", "Core", "Data", "Services", "Tests"]
parent_folder = ".."
ignored_folders = [".git", "bin", "obj"]
ignored_csproj_lines = ["<ItemGroup>", "<Project Sdk", "<PropertyGroup>", "<CodeAnalysisRuleSet>", 
        "<DebugType>", "</ItemGroup>", "</Project>", "</PropertyGroup>", "<IncludeAssets>",
        "<PrivateAssets>", "</PackageReference>", "<TargetFramework>", "<IsPackable>", "<AspNetCoreHostingModel>"]
empty_string = ""
end_of_line = "\r\n"
if_word = "if ("
and_word = "&&"
or_word = "||"
else_word = "else"
else_if_word = "else if ("
foreach_word = "foreach ("
case_word = "case"
while_word = "while ("

################### Helpers ###################

class Fonts:
    BLUE = '\033[94m'
    NOFONT = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'
    GREEN = '\033[1m\033[92m'
    YELLOW = '\033[1m\033[93m'
    RED = '\033[1m\033[91m'

class Module:
    def __init__(self, project_name, direct_dependencies, nuget_packages):
        self.project_name = project_name
        self.direct_dependencies = direct_dependencies
        self.nuget_packages = nuget_packages

class Build:
    def __init__(self, time_elapsed, build_errors, warnings):
        self.warnings = warnings
        self.build_errors = build_errors
        self.time_elapsed = time_elapsed

def __get_files():
    files = []
    for root, directories, filenames in os.walk(os.curdir):
        for filename in filenames:
            if not (__folder_must_be_ignored(root) or filename == 'kickoff.py' or filename == 'script.py'):
                files.append(os.path.join(root, filename))
    return files

def __get_csproj_files():
    files = []
    for root, directories, filenames in os.walk(os.curdir):
        for filename in filenames:
            if filename.endswith('.csproj'):
                files.append(os.path.join(root, filename))
    return files

def __folder_must_be_ignored(folder_name):
    for folder in ignored_folders:
        if folder in folder_name:
            return True
    return False

def __trim_project_name(path):
    name = str(path).replace('<ProjectReference Include=".', '')
    name = name.replace(' />', '')
    name = name.replace('"', '')
    name = name.split('.').pop(3)
    return name

def __trim_nuget_name(path):
    name = str(path).replace('<PackageReference Include="', '')
    name = name.replace('/>', '')
    name = name.replace('>', '')
    name = name.replace('"', '')
    return name

################### Metrics ###################

# Cyclomatic complexity
def calculate_complexity():
    files = __get_files()
    complexity = 0
    for file_name in files:
        file = open(file_name, 'r')
        content = file.read()
        if bootstrap_name in content:
            complexity += content.count(if_word)
            complexity += content.count(and_word)
            complexity += content.count(or_word)
            complexity += content.count(else_if_word)
            complexity += content.count(else_word)
            complexity += content.count(foreach_word)
            complexity += content.count(case_word)
            complexity += content.count(while_word)
        file.close()
    return complexity

# Direct dependencies
def get_direct_dependencies():
    csproj_files = __get_csproj_files()
    modules = []
    for file_name in csproj_files:
        direct_dependencies = []
        nuget_packages = []
        file = open(file_name, 'r')
        content = file.readlines()
        for line in content:
            if (bootstrap_name in line):
                direct_dependencies.append(__trim_project_name(line))
            elif not any(line.strip().startswith(s) for s in ignored_csproj_lines):
                nuget_packages.append(__trim_nuget_name(line))
        modules.append(Module(__trim_project_name(
            file_name), direct_dependencies, nuget_packages))
        file.close()
    return modules

# Code coverage
def code_coverage():
    print('Running code coverage analysis...')
    os.chdir(bootstrap_name + ".Tests")
    test_command = 'dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/ ./NetCoreBootstrap.Tests.csproj >/dev/null 2>&1'
    coverage_report = 'reportgenerator -reports:"./TestResults/coverage.opencover.xml"  -targetdir:"./TestResults/Coverage/Reports" -reportTypes:TextSummary >/dev/null 2>&1'
    grep = 'grep "Line coverage" ./TestResults/Coverage/Reports/Summary.txt'
    os.system(test_command)
    os.system(coverage_report)
    coverage = os.popen(grep).read()
    os.chdir(parent_folder)
    print('Removing files')
    os.remove(os.curdir + "/" + bootstrap_name +
              ".Tests/TestResults/coverage.opencover.xml")
    os.remove(os.curdir + "/" + bootstrap_name +
              ".Tests/TestResults/Coverage/Reports/Summary.txt")
    print('Finished')
    return coverage

# Build data
def build_data():
    clean_command = 'dotnet clean | '
    build_command = 'dotnet build '
    warnings = '| grep -e "Warning(s)" '
    errors = '-e "Error(s)" '
    time = '-e "Time Elapsed"'
    result = os.popen(clean_command + build_command +
                      warnings + errors + time).readlines()
    data = Build(result.pop().strip(),
                 result.pop().strip(), result.pop().strip())
    return data

# Main function
def run():
    complexity = calculate_complexity()
    direct_dependencies = get_direct_dependencies()
    coverage = float(code_coverage().strip().split(
        ' ').pop(2).replace('%', ''))
    data = build_data()
    print(Fonts.BOLD + Fonts.BLUE + 'Results:' + Fonts.NOFONT)
    if complexity > 50:
        print(Fonts.BOLD + 'Cyclomatic complexity: ' + Fonts.YELLOW +
              str(calculate_complexity()) + ' ---> High complexity')
    else:
        print(Fonts.BOLD + 'Cyclomatic complexity: ' +
              Fonts.GREEN + str(calculate_complexity()))
    print(Fonts.NOFONT + Fonts.BOLD + 'Direct dependencies:' + Fonts.NOFONT)
    for dep in direct_dependencies:
        print('\t- Module: ' + str(dep.project_name.upper()))
        for d in dep.direct_dependencies:
            print('\t\t- Depends on: ' + str(d).strip())
        if not dep.direct_dependencies:
            print('\t\tNo direct dependencies')
        for nuget in dep.nuget_packages:
            print('\t\t- Has nuget package: ' + str(nuget).strip())
    if coverage < 75:
        print(Fonts.BOLD + 'Line coverage: ' +
              Fonts.RED + str(coverage) + '%' + Fonts.NOFONT)
    else:
        print(Fonts.BOLD + 'Line coverage: ' +
              Fonts.GREEN + str(coverage) + '%' + Fonts.NOFONT)
    print(Fonts.BOLD + 'Build data:' + Fonts.NOFONT)
    print('\t' + data.warnings)
    print('\t' + data.build_errors)
    print('\t' + data.time_elapsed)

run()
