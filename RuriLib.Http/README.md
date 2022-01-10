# RuriLib.Http
This is a library that provides a custom HTTP client, in addition to an `HttpMessageHandler` to be used with the default `HttpClient` of `System.Net`. It sits on top of [RuriLib.Proxies](https://github.com/openbullet/OpenBullet2/tree/master/RuriLib.Proxies) which provides a layer 4 proxied connection.

# Installation
[NuGet](https://nuget.org/packages/RuriLib.Http): `dotnet add package RuriLib.Http`

# Example (Custom Client)
```csharp
using RuriLib.Http;
using RuriLib.Http.Models;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpDemo
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
            // Set up the proxy client (see RuriLib.Proxies documentation, here we use
            // a NoProxyClient for simplicity)
            var settings = new ProxySettings();
            var proxyClient = new NoProxyClient(settings);

            // Create the custom proxied client
            using var client = new RLHttpClient(proxyClient);

            // Create the request
            using var request = new HttpRequest
            {
                Uri = new Uri("https://httpbin.org/anything"),
                Method = HttpMethod.Post,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer ey..." }
                },
                Cookies = new Dictionary<string, string>
                {
                    { "PHPSESSID", "12345" }
                },

                // Content a.k.a. the "post data"
                Content = new StringContent("My content", Encoding.UTF8, "text/plain")
            };

            // Send the request and get the response (this can fail so make sure to wrap it in a try/catch block)
            using var response = await client.SendAsync(request);

            // Read and print the content of the response
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
    }
}
```

# Example (HttpClient)
This example is equivalent to the one above, but it uses the default `HttpClient` so you can take advantage of the API you are already familiar with.

```csharp
using RuriLib.Http;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpDemo
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
            // Set up the proxy client (see RuriLib.Proxies documentation, here we use
            // a NoProxyClient for simplicity)
            var settings = new ProxySettings();
            var proxyClient = new NoProxyClient(settings);

            // Create the handler that will be passed to HttpClient
            var handler = new ProxyClientHandler(proxyClient)
            {
                // This adds cookie support
                CookieContainer = new CookieContainer()
            };

            // Create the proxied HttpClient and the cookie container
            using var client = new HttpClient(handler);

            // Create the request
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://httpbin.org/anything"),
                Method = HttpMethod.Post,
                
                // Content a.k.a. the "post data"
                Content = new StringContent("My content", Encoding.UTF8, "text/plain")
            };

            request.Headers.TryAddWithoutValidation("Authorization", "Bearer ey...");
            handler.CookieContainer.Add(request.RequestUri, new Cookie("PHPSESSID", "12345"));

            // Send the request and get the response (this can fail so make sure to wrap it in a try/catch block)
            using var response = await client.SendAsync(request);

            // Read and print the content of the response
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
    }
}

```

# Credits
Some portions of the code were the work of Ruslan Khuduev and Artem Dontsov, to which I am grateful, all rights are reserved to them. Their work is under the MIT license.
