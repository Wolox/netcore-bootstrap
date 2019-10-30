using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;
using NetCoreBootstrap.Services;
using NetCoreBootstrap.Services.Helpers;
using NetCoreBootstrap.Services.Intefaces;
using Swashbuckle.AspNetCore.Swagger;

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
            if (CurrentEnvironment.IsEnvironment("Testing"))
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
                var connectionString = connectionStringBuilder.ToString();
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connection));
            }
            else
            {
                var connectionString = Configuration["ConnectionString"];
                services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(Configuration["ConnectionString"]));
            }
            services.AddScoped<DatabaseContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IMailer, Mailer>();
            services.AddSingleton<IAccountHelper, AccountHelper>();
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
            if (CurrentEnvironment.IsEnvironment("Development") || CurrentEnvironment.IsEnvironment("Staging"))
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Info { Title = ".NET Core Bootstrap API", Version = "v1" });
                    options.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter JWT with Bearer into field", Name = "Authorization", Type = "apiKey" });
                    options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", Enumerable.Empty<string>() } });
                });
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();
            app.UseAuthentication();
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DatabaseContext>();
                if (!CurrentEnvironment.IsEnvironment("Testing"))
                    context.Database.Migrate();
                else
                    context.Database.EnsureCreated();
            }
            if (CurrentEnvironment.IsEnvironment("Development") || CurrentEnvironment.IsEnvironment("Staging"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", ".NET Core Bootstrap API");
                });
            }
            app.UseMvc();
        }
    }
}
