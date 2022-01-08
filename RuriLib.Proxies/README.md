# RuriLib.Proxies
This is a library that can proxy a `TcpClient` via a proxy server. Supported protocols:
- HTTP
- SOCKS4
- SOCKS4a
- SOCKS5
- No proxy

All proxy clients derive from the same `ProxyClient` class, so it's really easy to implement different kinds of proxies (and even proxiless connections) with a simple switch statement in your application.

If you are planning to use this library to send HTTP requests via proxy servers, you should look into the [RuriLib.Http](https://github.com/openbullet/OpenBullet2/tree/master/RuriLib.Http) library, which depends on this. Only use this library if you are okay with working with raw TCP connections or if you are able to feed the `TcpClient` into another library that provides support for higher layer protocols.

# Installation
[NuGet](https://nuget.org/packages/RuriLib.Proxies): `dotnet add package RuriLib.Proxies`

# Example
```csharp
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = MainAsync(args);
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            // Initialize the proxy settings
            var settings = new ProxySettings
            {
                ConnectTimeout = TimeSpan.FromSeconds(10),
                ReadWriteTimeOut = TimeSpan.FromSeconds(30),
                Host = "127.0.0.1",
                Port = 8888,

                // Remove the following line if the proxy does not require authentication
                Credentials = new NetworkCredential("username", "password")
            };

            // Choose one of the following
            var httpProxyClient = new HttpProxyClient(settings); // HTTP proxies
            var socks4ProxyClient = new Socks4ProxyClient(settings); // Socks4 proxies
            var socks4aProxyClient = new Socks4aProxyClient(settings); // Socks4a proxies
            var socks5ProxyClient = new Socks5ProxyClient(settings); // Socks5a proxies
            var noProxyClient = new NoProxyClient(settings); // No proxy

            // Connect to the website via the proxy, we will get a TCP client that we can use
            using var tcpClient = await httpProxyClient.ConnectAsync("example.com", 80);

            // Now you can send messages on the raw TCP socket
            using var netStream = tcpClient.GetStream();
            using var memory = new MemoryStream();

            // Send HELLO
            var requestBytes = Encoding.ASCII.GetBytes("HELLO");
            await netStream.WriteAsync(requestBytes.AsMemory(0, requestBytes.Length));

            // Read the response
            await netStream.CopyToAsync(memory);
            memory.Position = 0;
            var data = memory.ToArray();
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }
    }
}
```

# Credits
Some portions of the code were the work of Ruslan Khuduev and Artem Dontsov, to which I am grateful, all rights are reserved to them. Their work is under the MIT license.
