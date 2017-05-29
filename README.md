## Install Net Core for Ubuntu 16.04 (link https://www.microsoft.com/net/core#linuxubuntu)
1. Add the dotnet apt-get feed
- (sudo sh -c 'echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ xenial main" > /etc/apt/sources.list.d/dotnetdev.list')
- (sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893)
- (sudo apt-get update)
2. Install .NET Core SDK
- (sudo apt-get install dotnet-dev-1.0.4)
3. Download Visual Studio Code
- https://code.visualstudio.com/Download?wt.mc_id=DotNet_Home

## clone repo seed
- (git clone git@github.com:gussiciliano/ASP-MV6-seed-project.git)

## Considerations
All routes should be declared through annotations (Eg: [Route("Home")]) so that they can be read correctly by Swagger. Also all routes should
have an annotation to indicate its request type (get, post, etc), otherwise swagger will default them to http get and parameters should have an annotation
indicating were they are coming from (body, query), otherwise swagger will default them to query parameters

## For Start new project MVC 6
1. create MVC project
- (dotnet new mvc -o MyMVCProject)
2. Restore the packages specified in the project file
- (dotnet restore)
3. Run project
- (dotnet run)
4. Add 'Models' folder
5. create new 'ApplicationDbContext.cs'