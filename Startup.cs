using System.Collections.Generic;
using System.IO;
using StaticWebServer.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StaticWebServer.Models;

namespace StaticWebServer
{
    public class Startup
    {
        
        private readonly StaticServer _frontEndServer = new StaticServer();

        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            var contentRoot = Configuration["ContentRootDirectory"] ?? "root";

            _frontEndServer.SetContentRoot(contentRoot);
            
            var myRectory = new FileInfo(this.GetType()
                    .Assembly.Location)
                .Directory.FullName;

            var proxyFilePath = Path.Join(myRectory, "Proxies.json");

            if (!File.Exists(proxyFilePath))
            {
                new ProxyList
                {
                    Proxies = new List<Proxy>
                    {
                        new Proxy
                        {
                            DisplayName = "Example",
                            TargetHost = "http://localhost/",
                            UriStarts = new List<string>()
                        }
                    }
                }.Save(proxyFilePath);
            }

            var proxyList = new ProxyList().Load(proxyFilePath);

            _frontEndServer.UseProxy(proxyList);

            var spaSupportString = Configuration["SpaSupport"]?.Trim().ToLower() ?? "true";

            var spaSupport = spaSupportString == "true" || spaSupportString == "yes";

            _frontEndServer.SupportSpa(spaSupport);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            
            _frontEndServer.ConfigurePreRouting(app, env);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            _frontEndServer.ConfigureMappings(app, env);
        }
    }
}
