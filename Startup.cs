using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;
using NetCoreBootstrap.Models.Database;
using Hangfire;
using Hangfire.PostgreSql;
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using LocalizationCultureCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;

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
            CultureInfo.CurrentCulture = new CultureInfo(Configuration["DefaultCulture"]);
            services.AddMvc().AddViewLocalization();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
            var connectionString = Configuration["ConnectionString"];
            services.AddDbContext<DataBaseContext>(options =>  options.UseNpgsql(connectionString));
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<DataBaseContext>() //ApplicationDbContext
                    .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options => {
                                                                options.LoginPath = "/Account/Login";
                                                                options.AccessDeniedPath = "/Account/AccessDenied";
                                                            });
            services.AddAuthentication().AddGoogle(googleOptions => {
                                                                googleOptions.ClientId = Configuration["GoogleAuth:ClientId"];
                                                                googleOptions.ClientSecret = Configuration["GoogleAuth:ClientSecret"];
                                                            });
            services.AddScoped<DataBaseContext>();
            services.AddHangfire(options => GlobalConfiguration.Configuration.UsePostgreSqlStorage(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)//, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            // Uncomment this if you want use Hangfire
            // app.UseHangfireDashboard();
            // app.UseHangfireServer(new BackgroundJobServerOptions(), null, new PostgreSqlStorage(Configuration["ConnectionString"]));
        }
    }
}
