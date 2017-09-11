NetCoreBootstrap
===============

Info for [NetCore](https://www.microsoft.com/net/core) applications.

## First steps

#### Installing NetCore

Get the latest version of NetCore following the guide in the [official website](https://www.microsoft.com/net/core).

### [Kickoff] Application Setup

After cloning the bootstrap, run `chmod +x ./Scripts/script.sh`
Then `./Scripts/script.sh AppName` where `AppName` is your application name.
If you don't need authentication run `./Scripts/script.sh AppName delete-auth`
Your app is ready. Happy coding!

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

By default, app will get settings from the file [appsetings.json](). Settings can be overwritten or extended by creating a file named `appsetings.ENV-NAME.json`, where `ENV-NAME` should be set as the value of the `EnvironmentName` environmental variable.

To set the Development enviroment, you should run the following command:
```bash
    export ASPNETCORE_ENVIRONMENT=Development
```
Then, you should create an apppsettings.Development.json file in order to store your database credentials. The file should look something like [this](https://gist.github.com/gzamudio/424f50d7ff3f1df6c12260b851f722b3)

#### Debugging

When developing a NetCore application in Linux or Mac you should use [Visual Studio Code](code.visualstudio.com) IDE, which already has excellent built-in debugging functionalities.
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

## Deploying to Heroku

1. Install Heroku CLI https://devcenter.heroku.com/articles/heroku-cli 
2. Log in to heroku with the folloing command:
```bash
    heroku login
```    
3. Create the heroku app with:
```bash
    heroku apps:create net-core-deploy-heroku
```
4. Set up the repository to point to your app
```bash
    heroku git:remote -a net-core-deploy-heroku
```
5. Add “Heroku Postgres” plugin from the app dashboard
6. Set the environment variable (ConnectionString) to create the connection string for your database. The database credentials can be obtained in https://data.heroku.com
7. Add the dotnetcore buildpack (https://github.com/jincod/dotnetcore-buildpack#v1.0.4) to the api from the dashboard’s settings page
8. Push your git repository with:
```bash
    git push heroku master
```
9. Database
If you’re using Entity Framework, you need to run the migrations using heroku bash. To do so, first run the command
```bash
    heroku run bash.
```
Then run the following commands:
```bash
    dotnet restore
    dotnet ef database update
```
Finally, you need to exit the heroku bash (using ctrl+d) and restart the heroku server so that the changes take place, with the command:
```bash
    heroku restart -a net-core-deploy-heroku
```

## Contributing

1. Fork it
2. Create your feature branch (`git checkout -b my-new-feature`)
3. Commit your changes (`git commit -am 'Add some feature'`)
4. Push to the branch (`git push origin my-new-feature`)
5. Create new Pull Request

## About

This project is maintained by [Gustavo Siciliano](https://github.com/gussiciliano) along with [Ignacio Torres](https://github.com/igna92ts) and [Michel Agopian](https://github.com/mishuagopian) and it was written by [Wolox](http://www.wolox.com.ar).

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
