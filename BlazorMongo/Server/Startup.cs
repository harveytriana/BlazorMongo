// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using BlazorMongo.Server.Services;
using BlazorMongo.Shared.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace BlazorMongo.Server
{
    public class Startup
    {
        public static string PATH { get; private set; }
        public static bool ISDEVELOPER { get; private set; }

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            PATH = env.ContentRootPath;

            ISDEVELOPER = env.IsDevelopment();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            // mongodb
            var connectionName = ISDEVELOPER ? "MongoNetWork" : "MongoAtlas";

            var mongoSettings = Configuration.GetSection(connectionName).Get<MongoSettings>();
            // collections
            services.AddSingleton<IMongoService<Book>>(MongoInitializer.Initialize<Book>(mongoSettings));

            // Swagger (by ilustration)
            if(ISDEVELOPER) {
                services.AddSwaggerGen(c => {
                    c.SwaggerDoc("v1", new OpenApiInfo {
                        Title = "Blazor Mongo API",
                        Version = "v1"
                    });
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();

                // Swagger http://localhost:port/apis
                app.UseSwagger();
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Axis.Api v1");
                    c.RoutePrefix = "apis";
                });
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
