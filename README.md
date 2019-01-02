NetCoreBootstrap
===============

Info for [NetCore](https://www.microsoft.com/net/core) applications.

## First steps

#### Installing NetCore

Get the latest version of NetCore following the guide in the [official website](https://www.microsoft.com/net/core).

### [Kickoff] Application Setup

After cloning the bootstrap, follow the [kickoff guide](https://github.com/Wolox/tech-guides/blob/master/net-core/docs/kickoff/README.md#kickoff).
And happy coding!

#### Getting dependencies

Run the following command from rootpath of the project to get the specified packages:.

```bash
    dotnet restore
```

#### Starting your app

Now, to start your app run `dotnet run` in the src path of the project. Then access your app at localhost:port. The port should be logged in the console.


#### Starting with wathcher

To enable auto restart run `dotnet watch run`


### Using SCSS
To use Scss files you need to run the following comands in the src path of the project:
```bash
    ./node_modules/.bin/gulp
```

If you don't have gulp run these comands:
```bash
    npm install gulp --save-dev
    npm install gulp-sass --save-dev
```


## Development

#### Environments & settings

By default, app will get settings from the file [appsetings.json](https://github.com/Wolox/netcore-bootstrap/blob/master/appsettings.json). Settings can be overwritten or extended by creating a file named `appsetings.ENV-NAME.json`, where `ENV-NAME` should be set as the value of the `EnvironmentName` environmental variable.

To set the Development enviroment, you should run the following command:
```bash
    export ASPNETCORE_ENVIRONMENT=Development
```
Then, you should create an apppsettings.Development.json file in order to store your database credentials. The file should look something like [this](https://gist.github.com/gzamudio/424f50d7ff3f1df6c12260b851f722b3)

#### Debugging

When developing a NetCore application in Linux or Mac you should use [Visual Studio Code](https://code.visualstudio.com/) IDE, which already has excellent built-in debugging functionalities.
To be able to debug, your launch.json file should look like [this](https://gist.github.com/gussiciliano/19b188e85d0ba95f04a0545ff12fbefd)
And make sure that you have this comand ```"command": "dotnet build"``` instead of this ```"command": "dotnet"``` on tasks.json 

#### Code First & Migrations

In this project we are using Code First development approach to populate and migrate our models in the database. It is required to have all models added in [DataBaseContext](./Models/Database/DataBaseContext.cs#L12) so that every time any of them gets modified [Entity Framework](https://docs.microsoft.com/en-us/ef/) can create the proper migration for them.

You can create a migration with the following command:

```bash
    dotnet ef migrations add DescriptiveNameForTheMigration
```

> *The first time you run this command, Entity Framework will create a snapshot of your models schema and will create the first migration to populate your tables. From this point, all new migrations will only populate migrations which change your DB schema to the current one.*

Finally, to update your database with migrations that have not run yet, you can execute:

```bash
    dotnet ef database update
```

#### Routes

Routes must be declared through annotations so that Swagger can generate documentation for them, like shown [here](./Controllers/api/v1/HomeApiController.cs).

> *See how HTTP verbs are also declared for endpoints.*

#### Scaffolding
In order to do scaffolding, it is necessary to have created both the model we want to scaffold and have ran the migrations. Once that's ready, you can to run the following command:
```bash
    dotnet aspnet-codegenerator controller -name ControllerName -m ModelName -dc DataBaseContext  --relativeFolderPath Controllers --useDefaultLayout --referenceScriptLibraries
```
This will generate the controler for that model, along with Create, Edit, Delete and Details methods and their respective views.

#### Async Jobs

To create asynchronous jobs implement [Hangfire](https://www.hangfire.io).

## Authentication

#### Identity

In order to add authentication in our application, we use Identity. For this, we must add the Identity Package:

```bash
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 2.0.0
```

After installing it, we need to configure it. First, we have to create a User model that inherits from IdentityUser, for example:

```bash
    public class ApplicationUser : IdentityUser 
    {        
        public virtual ICollection<IdentityRole> Roles { get; set; }
    }
```
Then, in ```ConfigureServices``` method on ```Startup.cs```, we need to add:

```bash
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
```

Also in this method you can set the login and access denied path:

```bash
services.ConfigureApplicationCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
```

Finally, in ```Configure``` method:
```bash
    app.UseAuthentication();
```

#### External login

We can also configure external logins with Google, Facebook. OpenId and more. 
For example, to add Google Authentication, we have to add the package:

```bash
    dotnet add package Microsoft.AspNetCore.Authentication.Google --version 2.0.0
```

Then, we have to edit again the ```ConfigureServices``` method to uncomment the corresponding lines in the ```Startup.cs``` file:

```bash
services.AddAuthentication().AddGoogle(googleOptions => {
    googleOptions.ClientId = Configuration["Authentication:GoogleAuth:ClientId"];
    googleOptions.ClientSecret = Configuration["Authentication:GoogleAuth:ClientSecret"];
});
```

This will set the ClientId and ClientSecret, which should be taken from the 'appsettings.{Environment}.json'. An example of this file:
```bash
{
    "Logging": {
        "IncludeScopes": false,
        "LogLevel": {
            "Default": "Debug",
            "System": "Information",
            "Microsoft": "Information"
        }
    },
    "Authentication":{
        "GoogleAuth": {
            "ClientId": "...",
            "ProjectId": "...", 
            "auth_uri": "https://accounts.google.com/o/oauth2/auth",
            "TokenUri": "https://accounts.google.com/o/oauth2/token",
            "AuthProviderX509CertUrl": "https://www.googleapis.com/oauth2/v1/certs",
            "ClientSecret": "...",
            "RedirectUris": ["http://localhost:5000/signin-google"]
        }
    },
    "ConnectionString" : "..."
}
```

#### Auth0 Authentication

To use Auth0 in our API calls, we first need to register [here](https://auth0.com/signup).
An useful tutorial to set this up is available [here](https://auth0.com/blog/developing-web-apps-with-asp-dot-net-core-2-dot-0-and-react-part-1/?utm_source=twitter&utm_medium=sc&utm_campaign=aspnet2_reactjs)

In order to enable Auth0 in your application, we only need to uncomment the following lines:
```bash
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(options =>
    {
        options.Authority = Configuration["Authentication:Auth0:Authority"];
        options.Audience = Configuration["Authentication:Auth0:Audience"];
    });
```

This will set the Authority and Audience needed, which should be taken from the 'appsettings.{Environment}.json'. An example of this file:
```bash
{
    "Logging": {
        "IncludeScopes": false,
        "LogLevel": {
            "Default": "Debug",
            "System": "Information",
            "Microsoft": "Information"
        }
    },
    "Authentication":{
        "Auth0": {
            "Authority": "<YOUR_AUTH0_AUTHORITY>",
            "Audience": "<YOUR_AUTH0_AUDIENCE>", 
        }
    },
    "ConnectionString" : "..."
}
```

Make sure to use the ```[Authorize]``` with the controller methods that need to be called by an authenticated user.

That's it! Whenever an endpoint that requires authrization needs to be accessed, a user can request a token querying to the domain provided by Auth0. The token can be obtained using the application's client id, client secret and audience url.

## Mailer

To set up the mailer, it is only necessary to add the authentication information into the environment variables. To do this via the appsetings.Development.json file, the mailer config section should look something like this:
```bash
    "Mailer":
    {
        "Email":YOUR_APP_EMAIL
        "Username":YOUR_APP_EMAIL_USERNAME
        "Password":YOUR_APP_EMAIL_PASSWORD,
        "Host":EMAIL_HOST,
        "Port":EMAIL_PORT,
        "Name":EMAIL_NAME
    }
```
The mailer should be set up in the Startup.cs file. In order to use it, you should uncomment this line:
```bash
    services.AddSingleton<IMailer, Mailer>();
```
To send an email, you just need to call the method:
```bash
    SendMail(toAddress, subject, body, isHtml);
```

## Globalization

To globalize the application is necessary to add the Localization Culture Core package to the project, to do this just run:

```bash
    dotnet add package LocalizationCultureCore --version x.x.x
```

Then, we configure it on ```ConfigureServices``` method on ```Startup.cs``` by adding this line:

```bash
    services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
    services.AddMvc().AddViewLocalization();
```

And locate all the JSON files that the library uses, inside the folder called "Resources" in the project's root directory.
The second line allows us to use localization in views as well as in the controllers.

Finally, also in ```ConfigureServices``` method, we can set the default language that your application will use:
```bash
    CultureInfo.CurrentUICulture = new CultureInfo("en-US");
```

The source code can be found [here](https://github.com/Wolox/localization-culture-core)

## Testing
For testing, you should use the files in the NetCoreBootstrap.Tests folder.
They currently allow the use of a Test server, created based on the source files of the project.
If you want to add a test project to the test folder, you just need to position yourself in NetCoreBootstrap.Tests and run the following command:
```bash
    dotnet new xunit
```
That will create a xUnit Test Project.
For more information, please head [here](https://github.com/dotnet/docs/blob/master/docs/core/testing/unit-testing-with-dotnet-test.md)

## Docker

First you need to install Docker (https://docs.docker.com/engine/installation/linux/docker-ce/ubuntu/#install-docker-ce-1)
When you have installed Docker you just need to run the following comand to create the docker container
```bash
    docker build -t netcore-bootstrap .
```

With your docker container you can:

#### Deploy to Heroku
1. Install Heroku CLI https://devcenter.heroku.com/articles/heroku-cli

2. Log in to heroku with the folloing command:
    ```bash
        heroku login
        heroku container:login
    ```    
3. Create the heroku app with:
    ```bash
        heroku apps:create net-core-deploy-heroku
    ```
4. Tag the heroku target image
    ```bash
        docker tag <image-name> registry.heroku.com/<heroku-app-name>/web
    ```
5. Push the docker image to heroku
    ```bash
        docker push registry.heroku.com/<heroku-app-name>/web
    ```
6. Create a new release using the images pushed to the Container Registry
    ```bash
        heroku container:release web -a <heroku-app-name>
    ```
If you have trouble running this command, make sure you have the latest version of Heroku CLI.

#### Deploy to AWS

##### Build Docker image on AWS

AWS has the possibility of directly building a Docker image, allowing us to just push our code (with a valid Dockerfile). 

Before you start, make sure you have a [valid Dockerfile](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/create_deploy_docker_image.html#create_deploy_docker_image_dockerfile) (or just use the one provided in this bootstrap).

1. Download the [Elastic Beanstalk Client](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/eb-cli3-install.html)
    ```bash
        sudo pip install awsebcli
    ```

2. Configure your access credentials, creating a file in your root folder `~/.aws/credentials`. Make sure it has the following format:
    ```bash
        [profile_name]
        aws_access_key_id = your_access_key
        aws_secret_access_key = your_access_key_secret
    ```

3. Create a Dockerrun.aws.json file in the root directory of your application. It should look like this:
    ```bash
        {
        "AWSEBDockerrunVersion": "1",
        "Ports": [
            {
            "ContainerPort": "80"
            }
        ],
        "Volumes": [],
        "Logging": "/var/log/nginx"
        }
    ```

4. Initialize Elastic Beanstalk in your application. Make sure the ```profile_name``` is the same one defined on your credentials file:
    ```bash
        eb init --profile profile_name
    ```
    Here, you will be required to select the AWS region you will be deploying to, along with the EB container. You may also be required to select the Docker Version you will be using. 

5. Make sure your environment variables are configured. This can be done from your AWS console, just go to ```Configuration```, then ```Software```. Keep in mind that once you click on ```Apply```, your environemnt will be restarted.

6. Deploy your application with :
    ```bash
        eb deploy
    ```

    If you are required to select an environment, you can list them using:
    ```bash
        eb list
    ```

    And deploy using:
    ```bash
        eb deploy environment_name
    ```

## Rollbar

Rollbar is a tool that allows monitoring errors from your application.

To use Rollbar, you only need to put the access token provided by rollbar in the appsettings.json file.

Here is a sample of how that part of the file should look:

```bash
    "Rollbar": {
        "AccessToken": "YOUR_ACCESS_TOKEN_HERE",
        "Environment": "YOUR_ENVIRONMENT"
    }
```

You can also set up your access token and environments using an environment variable.

In this bootstrap, we have provided some initial configuration on the ```Startup.cs``` file. In order to use it, you just need to uncomment the ```Rollbar``` sections on the ```ConfigureServices``` and ```Configure``` methods. Make sure to also uncomment the specific methods defined at the end of the file.

## Code Coverage

At the moment, the best tool available to do Code Coverage with .NET Core on Linux is [MiniCover](https://github.com/lucaslorentz/minicover)

It is required to follow a couple steps before using the tool. The following is a build script taken from the [repository](https://github.com/lucaslorentz/minicover):

```shell
    dotnet restore
    dotnet build

    cd tools

    # Instrument assemblies inside 'test' folder to detect hits for source files inside 'src' folder
    dotnet minicover instrument --workdir ../ --assemblies test/**/bin/**/*.dll --sources src/**/*.cs 

    # Reset hits count in case minicover was run for this project
    dotnet minicover reset

    cd ..

    for project in test/**/*.csproj; do dotnet test --no-build $project; done

    cd tools

    # Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
    dotnet minicover uninstrument --workdir ../

    # Create html reports inside folder coverage-html
    dotnet minicover htmlreport --workdir ../ --threshold 90

    # Print console report
    # This command returns failure if the coverage is lower than the threshold
    dotnet minicover report --workdir ../ --threshold 90

    cd ..
```

## Contributing

1. Fork it
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create new Pull Request

## About

This project is maintained by [Gustavo Siciliano](https://github.com/gussiciliano) along with [Gonzalo Zamudio](https://github.com/gzamudio) and [Marcos Trucco](https://github.com/truccomarcos) and it was written by [Wolox](http://www.wolox.com.ar).

![Wolox](https://raw.githubusercontent.com/Wolox/press-kit/master/logos/logo_banner.png)

## License

**netcore-bootstrap** is available under the MIT [license](LICENSE.md).

    Copyright (c) 2017 Wolox

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
