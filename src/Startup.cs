using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Mail;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using NetCoreBootstrap.Repositories.Database;
using NetCoreBootstrap.Repositories.Interfaces;
// using Rollbar;
using Swashbuckle.AspNetCore.Swagger;

namespace NetCoreBootstrap
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment currentEnvironment)
        {
            this.Configuration = configuration;
            this.CurrentEnvironment = currentEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment CurrentEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            // Rollbar service start
            // ConfigureRollbarSingleton();
            // Rollbar service end
            services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            CultureInfo.CurrentUICulture = new CultureInfo(Configuration["DefaultCulture"]);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddViewLocalization();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "NetCoreBootstrap API", Version = "v1" });
            });
            if (CurrentEnvironment.IsEnvironment("Testing"))
            {
                // If Testing environment, set in memory database
                var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
                var connectionString = connectionStringBuilder.ToString();
                var connection = new SqliteConnection(connectionString);
                services.AddDbContext<DataBaseContext>(options => options.UseSqlite(connection));
            }
            else
            {
                var connectionString = Configuration["ConnectionString"];
                // if not, set the postgres database
                services.AddDbContext<DataBaseContext>(options => options.UseNpgsql(connectionString));
            }
            // Begin for Identity
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<DataBaseContext>()
                    .AddDefaultTokenProviders();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // ----------------------------------------------------------------------------------------
            // JWT auth
            // To use this con the controllers, add the [Authorize] tag on the methods that require auth
            // services.AddAuthentication().AddJwtBearer(options =>
            //    {
            //        options.Audience = Configuration["Jwt:Issuer"];
            //        options.TokenValidationParameters = new TokenValidationParameters()
            //        {
            //            ClockSkew = TimeSpan.FromMinutes(0),
            //            ValidateLifetime = true,
            //            ValidateIssuerSigningKey = true,
            //            ValidAudience = Configuration["Jwt:Issuer"],
            //            ValidIssuer = Configuration["Jwt:Issuer"],
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"])),
            //        };
            //    });
            // End JWT Auth
            // ----------------------------------------------------------------------------------------
            // Facebook Auth
            // services.AddAuthentication().AddFacebook(facebookOptions => {
            //                                 facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
            //                                 facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            //                             });
            // End Facebook Auth
            // ----------------------------------------------------------------------------------------
            // Google Auth
            // services.AddAuthentication().AddGoogle(googleOptions =>
            // {
            //     googleOptions.ClientId = Configuration["Authentication:GoogleAuth:ClientId"];
            //     googleOptions.ClientSecret = Configuration["Authentication:GoogleAuth:ClientSecret"];
            // });
            // End Google Auth
            // ----------------------------------------------------------------------------------------
            // Final for Identity
            services.AddScoped<DataBaseContext>();
            // Uncomment this if you want use Hangfire
            // services.AddHangfire(options => GlobalConfiguration.Configuration.UsePostgreSqlStorage(connectionString));
            // services.AddSingleton<IMailer, Mailer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            if (env.IsEnvironment("Testing"))
            {
                // Create Database
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetService<DataBaseContext>();
                    context.Database.OpenConnection(); // see Resource #2 link why we do this
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
            }
            loggerFactory.AddFile("Logs/NetCoreBootstrapLogs-{Date}.txt", LogLevel.Error);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseCookiePolicy();
            // Rollbar middelware start
            // app.UseRollbarMiddleware();
            // Rollbar middelware end
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetCoreBootstrap API V1");
            });
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DataBaseContext>();
                context.Database.Migrate();
            }
            // Uncomment this to use Mailer
            // Mailer.SetAccountConfiguration(Configuration);
            // Uncomment this if you want use Hangfire
            // app.UseHangfireDashboard();
            // app.UseHangfireServer(new BackgroundJobServerOptions(), null, new PostgreSqlStorage(Configuration["ConnectionString"]));
        }

        // Rollbar methods start
        // private void ConfigureRollbarSingleton()
        // {
        //     string rollbarAccessToken = Configuration["Rollbar:AccessToken"];
        //     string rollbarEnvironment = Configuration["Rollbar:Environment"];
        //     RollbarLocator.RollbarInstance
        //         // minimally required Rollbar configuration:
        //         .Configure(new RollbarConfig(rollbarAccessToken) { Environment = rollbarEnvironment })
        //         // optional step if you would like to monitor Rollbar internal events within your application:
        //         .InternalEvent += OnRollbarInternalEvent;
        // }

        // private static void OnRollbarInternalEvent(object sender, RollbarEventArgs e)
        // {
        //     Console.WriteLine(e.TraceAsString());
        //     RollbarApiErrorEventArgs apiErrorEvent = e as RollbarApiErrorEventArgs;
        //     if (apiErrorEvent != null)
        //     {
        //         //TODO: handle/report Rollbar API communication error event...
        //         return;
        //     }
        //     CommunicationEventArgs commEvent = e as CommunicationEventArgs;
        //     if (commEvent != null)
        //     {
        //         //TODO: handle/report Rollbar API communication event...
        //         return;
        //     }
        //     CommunicationErrorEventArgs commErrorEvent = e as CommunicationErrorEventArgs;
        //     if (commErrorEvent != null)
        //     {
        //         //TODO: handle/report basic communication error while attempting to reach Rollbar API service...
        //         return;
        //     }
        //     InternalErrorEventArgs internalErrorEvent = e as InternalErrorEventArgs;
        //     if (internalErrorEvent != null)
        //     {
        //         //TODO: handle/report basic internal error while using the Rollbar Notifier...
        //         return;
        //     }
        // }
        // Rollbar methods end
    }
}
