using Yove.Proxy;
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
using RuriLib.Helpers;

namespace RuriLib.Blocks.Puppeteer.Browser
{
    [BlockCategory("Browser", "Blocks for interacting with a puppeteer browser", "#e9967a")]
    public static class Methods
    {
        [Block("Opens a new puppeteer browser", name = "Open Browser")]
        public static async Task PuppeteerOpenBrowser(BotData data, string extraCmdLineArgs = "")
        {
            data.Logger.LogHeader();

            // Check if there is already an open browser
            var oldBrowser = data.TryGetObject<PuppeteerSharp.Browser>("puppeteer");
            if (oldBrowser is not null && !oldBrowser.IsClosed)
            {
                data.Logger.Log("The browser is already open, close it if you want to open a new browser", LogColors.DarkSalmon);
                return;
            }

            var args = data.ConfigSettings.BrowserSettings.CommandLineArgs;

            // Extra command line args (to have dynamic args via variables)
            if (!string.IsNullOrWhiteSpace(extraCmdLineArgs))
            {
                args += ' ' + extraCmdLineArgs;
            }

            // If it's running in docker, currently it runs under root, so add the --no-sandbox otherwise chrome won't work
            if (Utils.IsDocker())
            {
                args += " --no-sandbox";
            }

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
                    data.SetObject("puppeteer.yoveproxy", proxyClient);
                    args += $" --proxy-server={proxyClient.GetProxy(null).Authority}";
                }
            }

            // Configure the options
            var launchOptions = new LaunchOptions
            {
                Args = new string[] { args },
                ExecutablePath = data.Providers.PuppeteerBrowser.ChromeBinaryLocation,
                IgnoredDefaultArgs = new string[] { "--disable-extensions", "--enable-automation" },
                Headless = data.ConfigSettings.BrowserSettings.Headless,
                DefaultViewport = null // This is important
            };

            // Add the plugins
            var extra = new PuppeteerExtra();
            extra.Use(new StealthPlugin());

            // Launch the browser
            var browser = await extra.LaunchAsync(launchOptions);
            browser.IgnoreHTTPSErrors = data.ConfigSettings.BrowserSettings.IgnoreHttpsErrors;

            // Save the browser for further use
            data.SetObject("puppeteer", browser);
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
            => data.TryGetObject<PuppeteerSharp.Browser>("puppeteer") ?? throw new Exception("The browser is not open!");

        private static PuppeteerSharp.Page GetPage(BotData data)
            => data.TryGetObject<PuppeteerSharp.Page>("puppeteerPage") ?? throw new Exception("No pages open!");

        private static void SwitchToMainFramePrivate(BotData data)
            => data.SetObject("puppeteerFrame", GetPage(data).MainFrame);

        private static void SetPageAndFrame(BotData data, PuppeteerSharp.Page page)
        {
            data.SetObject("puppeteerPage", page, false);
            SwitchToMainFramePrivate(data);
        }

        private static void StopYoveProxyInternalServer(BotData data)
            => data.TryGetObject<ProxyClient>("puppeteer.yoveproxy")?.Dispose();

        private static async Task SetPageLoadingOptions(BotData data, PuppeteerSharp.Page page)
        {
            await page.SetRequestInterceptionAsync(true);
            page.Request += (sender, e) =>
            {
                // If we only want documents and scripts but the resource is not one of those, block
                if (data.ConfigSettings.BrowserSettings.LoadOnlyDocumentAndScript && 
                    e.Request.ResourceType != ResourceType.Document && e.Request.ResourceType != ResourceType.Script)
                {
                    e.Request.AbortAsync();
                }

                // If the url contains one of the blocked urls
                else if (data.ConfigSettings.BrowserSettings.BlockedUrls
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Any(u => e.Request.Url.Contains(u, StringComparison.OrdinalIgnoreCase)))
                {
                    e.Request.AbortAsync();
                }

                // Otherwise all good, continue
                else
                {
                    e.Request.ContinueAsync();
                }
            };

            if (data.ConfigSettings.BrowserSettings.DismissDialogs)
            {
                page.Dialog += (sender, e) =>
                {
                    data.Logger.Log($"Dialog automatically dismissed: {e.Dialog.Message}", LogColors.DarkSalmon);
                    e.Dialog.Dismiss();
                };
            }
        }
    }
}
