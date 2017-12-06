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

Now, to start your app run `dotnet run` in the rootpath of the project. Then access your app at localhost:port. The port should be logged in the console.


#### Starting with wathcher

To enable auto restart run `dotnet watch run`


### Using SCSS
To use Scss files you need to run the following comands:
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

Then, we have to edit again the ```ConfigureServices``` method to add:

```bash
services.AddAuthentication().AddGoogle(googleOptions => {
    googleOptions.ClientId = Configuration["GoogleAuth:ClientId"];
    googleOptions.ClientSecret = Configuration["GoogleAuth:ClientSecret"];
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
  "GoogleAuth": {
		"ClientId": "...",
		"ProjectId": "...", 
        "auth_uri": "https://accounts.google.com/o/oauth2/auth",
		"TokenUri": "https://accounts.google.com/o/oauth2/token",
        "AuthProviderX509CertUrl": "https://www.googleapis.com/oauth2/v1/certs",
		"ClientSecret": "...",
		"RedirectUris": ["http://localhost:5000/signin-google"]
  },
  "ConnectionString" : "..."
}
```

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
    Mailer.SetAccountConfiguration(Configuration);
```
To send an email, you just need to call the static class:
```bash
    Mailer.Send(toAddress, subject, body);
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
    CultureInfo.CurrentCulture = new CultureInfo("en-US");
```

The source code can be found [here](https://github.com/Wolox/localization-culture-core)

## Deploying to Heroku

1. Install Heroku CLI https://devcenter.heroku.com/articles/heroku-cli
2. Install Docker https://docs.docker.com/engine/installation/linux/docker-ce/ubuntu/#install-docker-ce-1 
3. Publish your app
```bash
    dotnet publish -c Release
```
4. Copy the DockerFile to your publish directory

    Your publish directory should be:
```bash
    ./bin/release/netcoreapp2.0/publish
```
5. Build the Docker Image
```bash
    docker build -t <image-name> ./bin/release/netcoreapp2.0/publish
```
6. Log in to heroku with the folloing command:
```bash
    heroku login
    heroku container:login
```    
7. Create the heroku app with:
```bash
    heroku apps:create net-core-deploy-heroku
```
8. Tag the heroku target image
```bash
    docker tag <image-name> registry.heroku.com/<heroku-app-name>/web
```
9. Push the docker image to heroku
```bash
    docker push registry.heroku.com/<heroku-app-name>/web
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
