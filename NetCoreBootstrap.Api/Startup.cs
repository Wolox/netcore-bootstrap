using System;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;
using NetCoreBootstrap.Services;
using NetCoreBootstrap.Services.Intefaces;

namespace NetCoreBootstrap.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment currentEnvironment)
        {
            Configuration = configuration;
            CurrentEnvironment = currentEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Mailer>(Configuration.GetSection("Mailer"));
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization();
            services.AddJsonLocalization(options => options.ResourcesPath = "Resources");
            CultureInfo.CurrentUICulture = new CultureInfo(Configuration["DefaultCulture"]);
            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(Configuration["ConnectionString"]));
            services.AddScoped<DatabaseContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IMailer, Mailer>();
            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<DatabaseContext>()
                    .AddDefaultTokenProviders();
            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Audience = Configuration["Jwt:Issuer"];
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ClockSkew = TimeSpan.FromMinutes(0),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = Configuration["Jwt:Issuer"],
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"])),
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
