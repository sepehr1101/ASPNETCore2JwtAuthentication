﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthServer.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .ConfigureAppConfiguration((hostingContext, config) =>
                        {
                            var env = hostingContext.HostingEnvironment;
                            config.SetBasePath(env.ContentRootPath);
                            config.AddInMemoryCollection(new[]
                                {
                                    new KeyValuePair<string,string>("the-key", "the-value")
                                })
                                .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
                                .AddJsonFile($"appsettings.{env}.json", optional: true)
                                .AddEnvironmentVariables();
                        })
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            logging.AddDebug();
                            logging.AddConsole();
                        })
                        .UseSetting("detailedErrors", "true")
                        .CaptureStartupErrors(true)
                        .UseIISIntegration()
                        .UseDefaultServiceProvider((context, options) =>
                        {
                            options.ValidateScopes = context.HostingEnvironment.IsProduction();//IsDevelopment()//IsProduction()
                        })                        
                        .UseStartup<Startup>()
                        .Build();
            host.Run();
        }
    }
}
