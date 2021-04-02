﻿using Yove.Proxy;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ProxyType = RuriLib.Models.Proxies.ProxyType;

namespace RuriLib.Blocks.Puppeteer.Browser
{
    [BlockCategory("Browser", "Blocks for interacting with a puppeteer browser", "#e9967a")]
    public static class Methods
    {
        [Block("Opens a new puppeteer browser", name = "Open Browser")]
        public static async Task PuppeteerOpenBrowser(BotData data)
        {
            data.Logger.LogHeader();
         
            // Check if there is already an open browser
            if (data.Objects.ContainsKey("puppeteer") && !((PuppeteerSharp.Browser)data.Objects["puppeteer"]).IsClosed)
            {
                data.Logger.Log("The browser is already open, close it if you want to open a new browser", LogColors.DarkSalmon);
                return;
            }

            var args = data.ConfigSettings.PuppeteerSettings.CommandLineArgs;
            if (data.Proxy != null && data.UseProxy)
            {
                if (data.Proxy.Type == ProxyType.Http || !data.Proxy.NeedsAuthentication)
                {
                    args += $" --proxy-server={data.Proxy.Type.ToString().ToLower()}://{data.Proxy.Host}:{data.Proxy.Port}";
                }
                else
                {
                    var proxyType = data.Proxy.Type == ProxyType.Socks5 ? Yove.Proxy.ProxyType.Socks5 : Yove.Proxy.ProxyType.Socks4;
                    var proxyClient = new ProxyClient(
                        data.Proxy.Host, data.Proxy.Port,
                        data.Proxy.Username, data.Proxy.Password, 
                        proxyType);
                    data.Objects["puppeteer.yoveproxy"] = proxyClient;
                    args += $" --proxy-server={proxyClient.GetProxy(null).Authority}";
                }
            }

            // Configure the options
            var launchOptions = new LaunchOptions
            {
                Args = new string[] { args },
                ExecutablePath = data.Providers.PuppeteerBrowser.ChromeBinaryLocation,
                Headless = data.ConfigSettings.PuppeteerSettings.Headless,
                DefaultViewport = null // This is important
            };

            // Add the plugins
            var extra = new PuppeteerExtra();
            extra.Use(new StealthPlugin());

            // Launch the browser
            var browser = await extra.LaunchAsync(launchOptions);
            browser.IgnoreHTTPSErrors = data.ConfigSettings.PuppeteerSettings.IgnoreHttpsErrors;

            // Save the browser for further use
            data.Objects["puppeteer"] = browser;
            var page = (await browser.PagesAsync()).First();
            SetPageAndFrame(data, page);
            await SetPageLoadingOptions(data, page);
            // Authenticate if the proxy requires auth
            if (data.UseProxy && data.Proxy is { NeedsAuthentication: true, Type: ProxyType.Http } proxy)
                await page.AuthenticateAsync(new Credentials { Username = proxy.Username, Password = proxy.Password });

            data.Logger.Log($"{(launchOptions.Headless ? "Headless " : "")}Browser opened successfully!", LogColors.DarkSalmon);
        }

        [Block("Closes an open puppeteer browser", name = "Close Browser")]
        public static async Task PuppeteerCloseBrowser(BotData data)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            await browser.CloseAsync();
            StopYoveProxyInternalServer(data);
            data.Logger.Log("Browser closed successfully!", LogColors.DarkSalmon);
        }

        [Block("Opens a new page in a new browser tab", name = "New Tab")]
        public static async Task PuppeteerNewTab(BotData data)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            var page = await browser.NewPageAsync();
            await SetPageLoadingOptions(data, page);
            SetPageAndFrame(data, page); // Set the new page as active
            data.Logger.Log($"Opened a new page", LogColors.DarkSalmon);
        }

        [Block("Closes the currently active browser tab", name = "Close Tab")]
        public static async Task PuppeteerCloseTab(BotData data)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            var page = GetPage(data);
            
            // Close the page
            await page.CloseAsync();
            
            // Set the first page as active
            page = (await browser.PagesAsync()).FirstOrDefault();
            SetPageAndFrame(data, page);

            if (page != null)
                await page.BringToFrontAsync();

            data.Logger.Log($"Closed the active page", LogColors.DarkSalmon);
        }

        [Block("Switches to the browser tab with a specified index", name = "Switch to Tab")]
        public static async Task PuppeteerSwitchToTab(BotData data, int index)
        {
            data.Logger.LogHeader();

            var browser = GetBrowser(data);
            var page = (await browser.PagesAsync())[index];
            await page.BringToFrontAsync();
            SetPageAndFrame(data, page);

            data.Logger.Log($"Switched to tab with index {index}", LogColors.DarkSalmon);
        }

        [Block("Reloads the current page", name = "Reload")]
        public static async Task PuppeteerReload(BotData data)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            await page.ReloadAsync();
            SwitchToMainFramePrivate(data);

            data.Logger.Log($"Reloaded the page", LogColors.DarkSalmon);
        }

        [Block("Goes back to the previously visited page", name = "Go Back")]
        public static async Task PuppeteerGoBack(BotData data)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            await page.GoBackAsync();
            SwitchToMainFramePrivate(data);

            data.Logger.Log($"Went back to the previously visited page", LogColors.DarkSalmon);
        }

        [Block("Goes forward to the next visited page", name = "Go Forward")]
        public static async Task PuppeteerGoForward(BotData data)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            await page.GoForwardAsync();
            SwitchToMainFramePrivate(data);

            data.Logger.Log($"Went forward to the next visited page", LogColors.DarkSalmon);
        }

        private static PuppeteerSharp.Browser GetBrowser(BotData data)
            => (PuppeteerSharp.Browser)data.Objects["puppeteer"] ?? throw new Exception("The browser is not open!");

        private static PuppeteerSharp.Page GetPage(BotData data)
            => (PuppeteerSharp.Page)data.Objects["puppeteerPage"] ?? throw new Exception("No pages open!");

        private static void SwitchToMainFramePrivate(BotData data)
            => data.Objects["puppeteerFrame"] = GetPage(data).MainFrame;

        private static void SetPageAndFrame(BotData data, PuppeteerSharp.Page page)
        {
            data.Objects["puppeteerPage"] = page;
            SwitchToMainFramePrivate(data);
        }

        private static void StopYoveProxyInternalServer(BotData data)
        {
            if (data.Objects.GetValueOrDefault("puppeteer.yoveproxy") is ProxyClient proxyClient)
            {
                proxyClient.Dispose();
            }
        }

        private static async Task SetPageLoadingOptions(BotData data, PuppeteerSharp.Page page)
        {
            await page.SetRequestInterceptionAsync(true);
            page.Request += (sender, e) =>
            {
                if (e.Request.ResourceType == ResourceType.Document && data.ConfigSettings.PuppeteerSettings.LoadDocument)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Document && data.ConfigSettings.PuppeteerSettings.LoadDocument)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.StyleSheet && data.ConfigSettings.PuppeteerSettings.LoadStylesheet)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Image && data.ConfigSettings.PuppeteerSettings.LoadImage)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Media && data.ConfigSettings.PuppeteerSettings.LoadMedia)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Font && data.ConfigSettings.PuppeteerSettings.LoadFont)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Script && data.ConfigSettings.PuppeteerSettings.LoadScript)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.TextTrack && data.ConfigSettings.PuppeteerSettings.LoadTexttrack)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Xhr && data.ConfigSettings.PuppeteerSettings.LoadXhr)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Fetch && data.ConfigSettings.PuppeteerSettings.LoadFetch)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.EventSource && data.ConfigSettings.PuppeteerSettings.LoadEventsource)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.WebSocket && data.ConfigSettings.PuppeteerSettings.LoadWebsocket)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Other && data.ConfigSettings.PuppeteerSettings.LoadOther)
                {
                    e.Request.ContinueAsync();
                }
                else
                {
                    e.Request.AbortAsync();
                }
            };
        }
    }
}
