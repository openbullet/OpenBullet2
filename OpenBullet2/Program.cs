using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenBullet2.Helpers;

namespace OpenBullet2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Write the disclaimer
            Console.WriteLine("Welcome to OpenBullet 2!");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
----------
DISCLAIMER
----------
Performing attacks on sites you do not own (or you do not have permission to test) is illegal!
The developer will not be held responsible for improper use of this software.
");

            Console.ForegroundColor = ConsoleColor.White;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.WriteLine("DO NOT CLOSE THIS WINDOW" + Environment.NewLine);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                })
                .ConfigureAppConfiguration((hostingContext, config) => 
                {
                    config
#if RELEASE
                        .AddJsonFile("appsettings.Release.json")
                        .AddCommandLine(args);
#else
                        .AddJsonFile("appsettings.Development.json");
#endif
                });
    }
}
