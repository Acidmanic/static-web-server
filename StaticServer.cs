using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Acidmanic.Utilities.Results;
using angular_server.Extensions;
using angular_server.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace angular_server
{
    public class StaticServer
    {
        private readonly string _servingDirectoryName;

        private readonly string _defaultFile;

        private string _frontDirectory;

        private string _indexFile;

        private bool _serveForAngular = false;


        private ProxyList _proxyList = new ProxyList();


        public StaticServer(string servingDirectoryName, string defaultFile)
        {
            _servingDirectoryName = servingDirectoryName;
            _defaultFile = defaultFile;
        }


        public StaticServer(string servingDirectoryName) : this(servingDirectoryName, "index.html")
        {
        }

        public StaticServer() : this("front-end")
        {
        }

        public StaticServer ServeForAnguler()
        {
            _serveForAngular = true;

            return this;
        }

        public StaticServer UseProxy(ProxyList proxies)
        {
            _proxyList = proxies;

            return this;
        }

        public void ConfigurePreRouting(IApplicationBuilder app, IHostEnvironment env)
        {
            _frontDirectory = Path.Combine(env.ContentRootPath, _servingDirectoryName);

            _indexFile = Path.Combine(_frontDirectory, _defaultFile);

            if (!Directory.Exists(_frontDirectory))
            {
                Directory.CreateDirectory(_frontDirectory);
            }

            if (!File.Exists(_indexFile))
            {
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


        private Result<Proxy> MatchedProxy(HttpContext context)
        {
            foreach (var proxy in _proxyList.Proxies)
            {
                foreach (var uriStart in proxy.UriStarts)
                {
                    var prefix = uriStart.StartsWith("/") ? "" : "/";

                    var starter = PathString.FromUriComponent(prefix + uriStart);

                    if (context.Request.Path.StartsWithSegments(starter))
                    {
                        return new Result<Proxy>(true, proxy);
                    }
                }
            }

            return new Result<Proxy>().FailAndDefaultValue();
        }


        public void ConfigureMappings(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", c => c.Response.WriteAsync(File.ReadAllText(_indexFile)));
            });


            app.Use(async (context, next) =>
            {
                var matched = MatchedProxy(context);

                if (matched)
                {
                    await PerformProxy(context, matched.Value);
                }
                else
                {
                    await next.Invoke();
                }
            });


            if (_serveForAngular)
            {
                app.Use(async (context, next) =>
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


        private async Task PerformProxy(HttpContext context, Proxy matchedValue)
        {
            var request = new HttpRequestMessage()
            {
                Method = context.Request.Method.ToHttpMethod(),
                RequestUri = new Uri(context.Request.Path.ToString(), UriKind.Relative),
            };

            request.Content = new StreamContent(context.Request.Body);

            foreach (var header in context.Request.Headers)
            {
                try
                {
                    request.Headers.Add(header.Key, header.Value.ToArray());
                }
                catch (Exception e)
                {
                    try
                    {
                        request.Content.Headers.Add(header.Key, header.Value.ToArray());
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(matchedValue.TargetHost)
            };

            var response = await httpClient.SendAsync(request);

            foreach (var header in response.Headers)
            {
                try
                {
                    context.Response.Headers.Add(header.Key, new StringValues(header.Value.ToString()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
            context.Response.StatusCode = (int)response.StatusCode;

            var responseContent = await response.Content.ReadAsByteArrayAsync();

            await context.Response.Body.WriteAsync(responseContent);
        }
    }
}