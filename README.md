## Install Net Core for Ubuntu 16.04 (link https://www.microsoft.com/net/core#linuxubuntu)
1. Add the dotnet apt-get feed
- sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
- sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
- sudo apt-get update
2. Install .NET Core SDK
- sudo apt-get install dotnet-dev-1.0.4
3. Download Visual Studio Code
- https://code.visualstudio.com/Download?wt.mc_id=DotNet_Home


## Clone repo seed and create DataBase
- git clone git@github.com:gussiciliano/ASP-MV6-seed-project.git
- sudo -u postgres psql
- CREATE ROLE "test_asp" LOGIN CREATEDB PASSWORD 'test_asp';
- \q


## Run project
1. Restore the packages specified in the project file
- dotnet restore
2. In this project there are an User model, this create an user table in BD, if you want create other tables follow next steps:
- Remove User.cs from Model->DataBasa and his references examples
- Create your models, if you needed
- Run 'dotnet ef migrations remove'
- Run 'dotnet ef migrations add InitialMigration'
3. Create DataBase and use lunch.json for env vars
- dotnet ef database update 
4. Run project
- dotnet run


## Considerations
All routes should be declared through annotations (Eg: [Route("Home")]) so that they can be read correctly by Swagger. Also all routes should have an annotation to indicate its request type (get, post, etc), otherwise swagger will default them to http get and parameters should have an annotation indicating were they are coming from (body, query), otherwise swagger will default them to query parameters


## For Start new project MVC 6
1. create MVC project
- (dotnet new mvc -o MyMVCProject)
** If you need persistence follow next steps
2. Configure connectionString and use lunch.json for env vars
3. Add 'Models' and 'DataBase' folders
4. create 'DataBaseContext.cs'
5. create necesary 'Entities' class
6. change your Startup.cs for First Code method
7. Create migartions
- dotnet ef migrations add InitialMigration
8. Update Data Base
- dotnet ef database update

# Use Jobs
- For use Job we implement Hangfire (https://www.hangfire.io/)

# For use Json
- JsonConvert.SerializeObject(Object)

# For code analysis on aspnet (Work in progress)
http://dotnetthoughts.net/enable-code-analysis-on-aspnet-core-applications/
