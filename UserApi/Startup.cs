using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserApi
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Set the comments path for the Swagger JSON and UI.
            // I know, is redundant BUT it may be usefull later...
            string apiName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string xmlFile = apiName + ".xml";
            string xmlPath;
            try
            {
                string slnPath = System.IO.Directory.GetParent(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf(apiName))).ToString();
                string apiPath = System.IO.Path.Combine(slnPath, apiName);
                xmlPath = System.IO.Path.Combine(apiPath + "/bin", xmlFile);
            }
            catch (System.Exception)
            {
                xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            }

            services.AddCors();

            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            #region DataContext
            string connectionString = Configuration.GetConnectionString("LocalMySQL80");
            System.Console.WriteLine("connectionString: " + connectionString);

            /// <summary>
            /// Add Database Context using EF Core InMemory Database
            /// This is mainly for testing purposes only!
            /// Always comment this instruction when publishing the API (Pushing to GitHub)
            /// </summary>
            // services.AddDbContext<Users.Domain.Infra.Contexts.UsersContext>(opt => opt.UseInMemoryDatabase("Database"));

            /// <summary>
            /// Add Database Context using Pomelo EF Core MySql
            /// Use any valid connection string at appsettings.json
            /// </summary>
            string clientDB = Configuration.GetValue<string>("ClientId");
            var version = new Version(8, 0, 19);
            if (clientDB == "natureinvest")
            {
                version = new Version(5, 7, 0);
            }
            services.AddDbContext<Users.Domain.Infra.Contexts.UsersContext>(options => options
                .UseMySql(connectionString, mySqlOptions => mySqlOptions
                .ServerVersion(new ServerVersion(version))
                .MigrationsAssembly("Users.Domain.Infra"))
                // .EnableSensitiveDataLogging()
                );
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
