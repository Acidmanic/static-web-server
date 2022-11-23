using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace angular_server
{
    public class StaticServer
    {

        private readonly string _servingDirectoryName;

        private readonly string _defaultFile;

        private  string _frontDirectory;
        
        private  string _indexFile;

        private bool _serveForAngular = false;
        
        public StaticServer(string servingDirectoryName, string defaultFile)
        {
            _servingDirectoryName = servingDirectoryName;
            _defaultFile = defaultFile;
        }

        
        public StaticServer(string servingDirectoryName):this(servingDirectoryName,"index.html")
        {
            
        }

        public StaticServer():this("front-end")
        {
            
        }

        public StaticServer ServeForAnguler()
        {
            _serveForAngular = true;
            
            return this;
        }

        public void ConfigurePreRouting(IApplicationBuilder app, IHostEnvironment env)
        {
            _frontDirectory = Path.Combine(env.ContentRootPath, _servingDirectoryName);

            _indexFile = Path.Combine(_frontDirectory, _defaultFile);

            if (!Directory.Exists(_frontDirectory))
            {
                Directory.CreateDirectory(_frontDirectory);


                File.WriteAllText(_indexFile,
                    "<H1>Hello!, Apparently I'm being Updated! will be back soon! :D </H1>");
            }

            var fileProvider = new PhysicalFileProvider(_frontDirectory);


            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = "",
                ServeUnknownFileTypes = true
            });
            
        }


        public void ConfigureMappings(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", c => c.Response.WriteAsync(File.ReadAllText(_indexFile)));
            });

            if (_serveForAngular)
            {
                app.Use( async (context,next) =>
                {
                    await next.Invoke();
                
                    if (context.Response.StatusCode == 404)
                    {
                        context.Response.StatusCode = 200;

                        var content = await File.ReadAllTextAsync(_indexFile);
                    
                        await context.Response.WriteAsync(content);
                    }
                
                });   
            }
        }
    }
}