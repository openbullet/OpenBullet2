using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenBullet2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Write the disclaimer
            Console.WriteLine(@"Welcome to OpenBullet 2.

==============
  DISCLAIMER
==============
Performing attacks on sites you do not own (or you do not have permission to test) is illegal!
The developer will not be held responsible for improper use of this software.
");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                });
    }
}
