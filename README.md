NetCore Bootstrap
===============

Kickoff for [NetCore](https://www.microsoft.com/net/core) applications.

## First steps

#### Installing NetCore

Get the latest version of NetCore following the guide in the [official website](https://www.microsoft.com/net/core).

#### Database configuration & secret keys

Before running the app, make sure you have a postgres db created. Then, you must configure its connection. To do so, we will use `Secret Keys` to avoid pushing our local (or production) database credentials, setting a secret named `SecretConnectionString` with the configuration, as the following:

```bash
    dotnet restore
    dotnet user-secrets set SecretConnectionString "ID=YOUR_USER_POSTGRES;Password=YOUR_PASS_POSTGRES;Host=YOUR_HOST;Port=5432;Database=YOUR_DATA_BASE;Pooling=true;"
```

If you want to see all secrets keys run

```bash
    dotnet user-secrets list
```

#### Getting dependencies

Run the following command from rootpath of the project to get the specified packages:.

```bash
    dotnet restore
```

#### Starting your app

Now, to start your app run `dotnet run` in the rootpath of the project. Then access your app at localhost:port. The port should be logged in the console.

## Development

#### Environments & settings

By default, app will get settings from the file [appsetings.json](). Settings can be overwritten or extended by creating a file named `appsetings.ENV-NAME.json`, where `ENV-NAME` should be set as the value of the `EnvironmentName` environmental variable.

#### Debugging

When developing a NetCore application users use [Visual Studio](www.visualstudio.com) or [Visual Studio Code](code.visualstudio.com) IDEs, which already have excellent built-in debugging functionalities.

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

#### Async Jobs

To create asynchronous jobs implement [Hangfire](https://www.hangfire.io).
