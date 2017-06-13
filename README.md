## Install Net Core for Ubuntu 16.04 (link https://www.microsoft.com/net/core#linuxubuntu)
1. Add the dotnet apt-get feed
```bash
    sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
    sudo apt-get update
```
2. Install .NET Core SDK
```bash
    sudo apt-get install dotnet-dev-1.0.4
```
3. Download Visual Studio Code
- https://code.visualstudio.com/Download?wt.mc_id=DotNet_Home


## Clone repo seed and create DataBase
```bash
    git clone git@github.com:gussiciliano/ASP-MV6-seed-project.git
    sudo -u postgres psql
    CREATE ROLE "test_asp" LOGIN CREATEDB PASSWORD 'test_asp';
    \q
```


## Create Secrets keys (ConnectionString)
For this we use Secret Manager tool (https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets#installing-the-secret-manager-tool)
```bash
    dotnet restore
    dotnet user-secrets set SecretConnectionString "ID=YOUR_USER_POSTGRES;Password=YOUR_PASS_POSTGRES;Host=YOUR_HOST;Port=5432;Database=YOUR_DATA_BASE;Pooling=true;"
```
If you want to see all secrets keys run
```bash
    dotnet user-secrets list
```

## Run project
1. Restore the packages specified in the project file
```bash
    dotnet restore
```
2. In this project there are an User model, this create an user table in BD, if you want create other tables follow next steps:
- Remove User.cs from Model->DataBasa and his references examples
- Create your models, if you needed
- Migrations
```bash
    dotnet ef migrations remove
    dotnet ef migrations add InitialMigration
```
3. Create DataBase with migrations
```bash
    dotnet ef database update 
```
4. Run project
```bash
    dotnet run
```


## Considerations
All routes should be declared through annotations (Eg: [Route("Home")]) so that they can be read correctly by Swagger. Also all routes should have an annotation to indicate its request type (get, post, etc), otherwise swagger will default them to http get and parameters should have an annotation indicating were they are coming from (body, query), otherwise swagger will default them to query parameters


## For Start new project MVC 6
1. create MVC project
```bash
    dotnet new mvc -o MyMVCProject)
```
** If you need persistence follow next steps
2. Configure connectionString and use lunch.json for env vars
3. Add 'Models' and 'DataBase' folders
4. create 'DataBaseContext.cs'
5. create necesary 'Entities' class
6. change your Startup.cs for First Code method
7. Create migartions
```bash
    dotnet ef migrations add InitialMigration
```
8. Update Data Base
```bash
    dotnet ef database update
```


## Use Jobs
- For use Job we implement Hangfire (https://www.hangfire.io/)


## For use Json
- JsonConvert.SerializeObject(Object)


## For code analysis on aspnet (Work in progress)
http://dotnetthoughts.net/enable-code-analysis-on-aspnet-core-applications/
