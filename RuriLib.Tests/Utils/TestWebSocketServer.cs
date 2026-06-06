using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal static class TestWebSocketServer
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private static WebApplication? app;
    private static string? baseUrl;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public static async Task<string> BuildEchoUrl()
        => $"{(await GetBaseUrl()).Replace("http://", "ws://", StringComparison.Ordinal)}/websocket/echo";

    private static async Task<string> GetBaseUrl()
    {
        await EnsureInitialized();
        return baseUrl!;
    }

    private static async Task EnsureInitialized()
    {
        if (baseUrl is not null)
        {
            return;
        }

        await Semaphore.WaitAsync(TestCancellationToken);
        try
        {
            if (baseUrl is not null)
            {
                return;
            }

            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));

            var webApp = builder.Build();
            webApp.UseWebSockets();

            webApp.MapGet("/get", () => Results.Ok(new { ok = true }));
            webApp.Map("/websocket/echo", EchoHandlerAsync);

            try
            {
                await webApp.StartAsync(TestCancellationToken);
            }
            catch
            {
                await webApp.DisposeAsync();
                throw;
            }

            var addresses = webApp.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()?
                .Addresses;

            baseUrl = addresses?.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                await webApp.DisposeAsync();
                throw new InvalidOperationException("The local websocket test server did not publish a listening address");
            }

            app = webApp;
            AppDomain.CurrentDomain.ProcessExit += DisposeAppOnProcessExit;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task EchoHandlerAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[4096];

        while (!context.RequestAborted.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            var offset = 0;

            do
            {
                var segment = new ArraySegment<byte>(buffer, offset, buffer.Length - offset);
                result = await webSocket.ReceiveAsync(segment, context.RequestAborted);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                    return;
                }

                offset += result.Count;
            }
            while (!result.EndOfMessage);

            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, offset),
                result.MessageType,
                true,
                context.RequestAborted);
        }
    }

    private static void DisposeAppOnProcessExit(object? sender, EventArgs e)
        => DisposeApp().GetAwaiter().GetResult();

    private static async Task DisposeApp()
    {
        if (app is null)
        {
            return;
        }

        try
        {
            await app.DisposeAsync();
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= DisposeAppOnProcessExit;
            app = null;
            baseUrl = null;
        }
    }
}
