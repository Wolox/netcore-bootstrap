using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Models.Database;
using Swashbuckle.AspNetCore.Swagger;

namespace NetCoreBootstrap
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            CultureInfo.CurrentUICulture = new CultureInfo(Configuration["DefaultCulture"]);
            services.AddMvc().AddViewLocalization();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "NetCoreBootstrap API", Version = "v1" });
            });
            var connectionString = Configuration["ConnectionString"];
            services.AddDbContext<DataBaseContext>(options => options.UseNpgsql(connectionString));
            // Begin for Identity
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<DataBaseContext>()
                    .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            // ----------------------------------------------------------------------------------------
            // JWT auth
            // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            // services.AddAuthentication(options =>
            // {
            //     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            // }).AddJwtBearer(cfg =>
            // {
            //     cfg.RequireHttpsMetadata = false;
            //     cfg.SaveToken = true;
            //     cfg.TokenValidationParameters = new TokenValidationParameters
            //     {
            //         ValidIssuer = Configuration["Authentication:Jwt:Issuer"],
            //         ValidAudience = Configuration["Authentication:Jwt:JwtIssuer"],
            //         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:JwtKey"])),
            //         ClockSkew = TimeSpan.Zero,
            //     };
            // });
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else app.UseExceptionHandler("/Home/Error");
            loggerFactory.AddFile("Logs/NetCoreBootstrapLogs-{Date}.txt", LogLevel.Error);
            app.UseStaticFiles();
            app.UseAuthentication();
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
    }
}
