using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using angular_server.Extensions;
using angular_server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace angular_server
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
            
            _frontEndServer.ServeForAngular();
            
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
