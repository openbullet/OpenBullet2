﻿using PuppeteerSharp;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Puppeteer.Page
{
    [BlockCategory("Page", "Blocks for interacting with a puppeteer browser page", "#e9967a")]
    public static class Methods
    {
        [Block("Navigates to a given URL in the current page", name = "Navigate To")]
        public static async Task PuppeteerNavigateTo(BotData data, string url = "https://example.com",  WaitUntilNavigation loadedEvent = WaitUntilNavigation.Load, string referer = "https://google.com", int timeout = 30000)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var options = new NavigationOptions
            {
                Timeout = timeout,
                Referer = referer,
                WaitUntil = new WaitUntilNavigation[] { loadedEvent }
            };
            var response = await page.GoToAsync(url, options);

            data.ADDRESS = response.Url;
            data.SOURCE = await response.TextAsync();
            data.RAWSOURCE = await response.BufferAsync();

            SwitchToMainFramePrivate(data);

            data.Logger.Log($"Navigated to {url}", LogColors.DarkSalmon);
        }

        [Block("Clears cookies in the page stored for a specific website", name = "Clear Cookies")]
        public static async Task PuppeteerClearCookies(BotData data, string website)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var cookies = await page.GetCookiesAsync(website);
            await page.DeleteCookieAsync(cookies);

            data.Logger.Log($"Cookies cleared for site {website}", LogColors.DarkSalmon);
        }

        [Block("Sends keystrokes to the browser page", name = "Type in Page")]
        public static async Task PuppeteerPageType(BotData data, string text)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);

            await page.Keyboard.TypeAsync(text);

            data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
        }

        [Block("Presses and releases a key in the browser page", name = "Key Press in Page", extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js")]
        public static async Task PuppeteerPageKeyPress(BotData data, string key)
        {
            data.Logger.LogHeader();   

            var page = GetPage(data);

            await page.Keyboard.PressAsync(key);

            data.Logger.Log($"Pressed and released {key}", LogColors.DarkSalmon);
            // Full list of keys: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js
        }

        [Block("Presses a key in the browser page without releasing it", name = "Key Down in Page", extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js")]
        public static async Task PuppeteerPageKeyDown(BotData data, string key)
        {
            data.Logger.LogHeader();  

            var page = GetPage(data);

            await page.Keyboard.DownAsync(key);

            data.Logger.Log($"Pressed (and holding down) {key}", LogColors.DarkSalmon);
            // Full list of keys: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js
        }

        [Block("Releases a key that was previously pressed in the browser page", name = "Key Up in Page", extraInfo = "Full list of keys here: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js")]
        public static async Task PuppeteerKeyUp(BotData data, string key)
        {
            data.Logger.LogHeader();  
            
            var page = GetPage(data);

            await page.Keyboard.UpAsync(key);

            data.Logger.Log($"Released {key}", LogColors.DarkSalmon);
            // Full list of keys: https://github.com/puppeteer/puppeteer/blob/v1.14.0/lib/USKeyboardLayout.js
        }

        [Block("Takes a screenshot of the entire browser page and saves it to an output file", name = "Screenshot Page")]
        public static async Task PuppeteerScreenshotPage(BotData data, string file, bool fullPage = false, bool omitBackground = false)
        {
            data.Logger.LogHeader();   

            var page = GetPage(data);
            var options = new ScreenshotOptions
            {
                FullPage = fullPage,
                OmitBackground = omitBackground,
                Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg
            };
            await page.ScreenshotAsync(file, options);

            data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page and saved it to {file}", LogColors.DarkSalmon);
        }

        [Block("Takes a screenshot of the entire browser page and converts it to a base64 string", name = "Screenshot Page Base64")]
        public static async Task<string> PuppeteerScreenshotPageBase64(BotData data, bool fullPage = false, bool omitBackground = false)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var options = new ScreenshotOptions
            {
                FullPage = fullPage,
                OmitBackground = omitBackground,
                Type = omitBackground ? ScreenshotType.Png : ScreenshotType.Jpeg
            };
            var base64 = await page.ScreenshotBase64Async(options);

            data.Logger.Log($"Took a screenshot of the {(fullPage ? "full" : "visible")} page as base64", LogColors.DarkSalmon);

            return base64;
        }

        [Block("Scrolls to the top of the page", name = "Scroll to Top")]
        public static async Task PuppeteerScrollToTop(BotData data)
        {
            data.Logger.LogHeader(); 

            var page = GetPage(data);

            await page.EvaluateExpressionAsync("window.scrollTo(0, 0);");

            data.Logger.Log($"Scrolled to the top of the page", LogColors.DarkSalmon);
        }

        [Block("Scrolls to the bottom of the page", name = "Scroll to Bottom")]
        public static async Task PuppeteerScrollToBottom(BotData data)
        {
            data.Logger.LogHeader();  

            var page = GetPage(data);

            await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");

            data.Logger.Log($"Scrolled to the bottom of the page", LogColors.DarkSalmon);
        }

        [Block("Scrolls the page by a certain amount horizontally and vertically", name = "Scroll by")]
        public static async Task PuppeteerScrollBy(BotData data, int horizontalScroll, int verticalScroll)
        {
            data.Logger.LogHeader();   
            
            var page = GetPage(data);

            await page.EvaluateExpressionAsync($"window.scrollBy({horizontalScroll}, {verticalScroll});");

            data.Logger.Log($"Scrolled by ({horizontalScroll}, {verticalScroll})", LogColors.DarkSalmon);
        }

        [Block("Sets the viewport dimensions and options", name = "Set Viewport")]
        public static async Task PuppeteerSetViewport(BotData data, int width, int height, bool isMobile = false, bool isLandscape = false, float scaleFactor = 1f)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var options = new ViewPortOptions
            {
                Width = width,
                Height = height,
                IsMobile = isMobile,
                IsLandscape = isLandscape,
                DeviceScaleFactor = scaleFactor
            };

            await page.SetViewportAsync(options);

            data.Logger.Log($"Set the viewport size to {width}x{height}", LogColors.DarkSalmon);
        }

        [Block("Gets the full DOM of the page", name = "Get DOM")]
        public static async Task<string> PuppeteerGetDOM(BotData data)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var dom = await page.EvaluateExpressionAsync<string>("document.body.innerHTML");

            data.Logger.Log($"Got the full page DOM", LogColors.DarkSalmon);
            data.Logger.Log(dom, LogColors.DarkSalmon, true);

            return dom;
        }

        [Block("Gets the cookies for a given domain from the browser", name = "Get Cookies")]
        public static async Task<Dictionary<string, string>> PuppeteerGetCookies(BotData data, string domain)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var cookies = await page.GetCookiesAsync(domain);

            data.Logger.Log($"Got {cookies.Length} cookies for {domain}", LogColors.DarkSalmon);

            return cookies.ToDictionary(c => c.Name, c => c.Value);
        }

        [Block("Sets the cookies for a given domain in the browser page", name = "Set Cookies")]
        public static async Task PuppeteerSetCookies(BotData data, string domain, Dictionary<string, string> cookies)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            await page.SetCookieAsync(cookies.Select(c => new CookieParam { Domain = domain, Name = c.Key, Value = c.Value }).ToArray());

            data.Logger.Log($"Set {cookies.Count} cookies for {domain}", LogColors.DarkSalmon);
        }

        [Block("Sets the User Agent of the browser page", name = "Set User-Agent")]
        public static async Task PuppeteerSetUserAgent(BotData data, string userAgent)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            await page.SetUserAgentAsync(userAgent);

            data.Logger.Log($"User Agent set to {userAgent}", LogColors.DarkSalmon);
        }

        [Block("Switches to the main frame of the page", name = "Switch to Main Frame")]
        public static void PuppeteerSwitchToMainFrame(BotData data)
        {
            data.Logger.LogHeader();

            SwitchToMainFramePrivate(data);

            data.Logger.Log($"Switched to main frame", LogColors.DarkSalmon);
        }

        [Block("Evaluates a js expression in the current page and returns a json response", name = "Execute JS")]
        public static async Task<string> PuppeteerExecuteJs(BotData data, string expression)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var response = await page.EvaluateExpressionAsync(expression);
            var json = response != null ? response.ToString() : "undefined";

            data.Logger.Log($"Evaluated {expression}", LogColors.DarkSalmon);
            data.Logger.Log($"Got result: {json}", LogColors.DarkSalmon);
            
            return json;
        }

        [Block("Capture the response from the given URL", name = "Wait For Response")]
        public static async Task PuppeteerWaitForResponse(BotData data, string url)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);
            var response = await page.WaitForResponseAsync(url);
            var headers = response.Headers;

            data.ADDRESS = response.Url;
            data.RESPONSECODE = (int)response.Status;
            data.HEADERS = headers;

            if (((int)response.Status) / 100 != 3)
            {
                data.SOURCE = await response.TextAsync();
                data.RAWSOURCE = await response.BufferAsync();
            }

            data.Logger.Log($"Address: {data.ADDRESS}", LogColors.DodgerBlue);
            data.Logger.Log($"Response code: {data.RESPONSECODE}", LogColors.Citrine);

            data.Logger.Log("Received Headers:", LogColors.MediumPurple);
            data.Logger.Log(data.HEADERS.Select(h => $"{h.Key}: {h.Value}"), LogColors.Violet);
        }

        [Block("block the loading of the resource type", name = "Set Request Interception")]
        public static async Task PuppeteerSetRequestInterceptionAsync(BotData data, bool loadDocument = true, bool loadStylesheet = true, bool loadImage = true, bool loadMedia = true, bool loadFont = true, bool loadScript = true, bool loadTexttrack = true, bool loadXhr = true, bool loadFetch = true, bool loadEventsource = true, bool loadWebsocket = true, bool loadOther = true)
        {
            data.Logger.LogHeader();

            var page = GetPage(data);

            await page.SetRequestInterceptionAsync(true);
            page.Request += (sender, e) =>
            {
                if (e.Request.ResourceType == ResourceType.Document && loadDocument)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == 0 && loadDocument)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.StyleSheet && loadStylesheet)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Image && loadImage)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Media && loadMedia)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Font && loadFont)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Script && loadScript)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.TextTrack && loadTexttrack)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Xhr && loadXhr)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Fetch && loadFetch)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.EventSource && loadEventsource)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.WebSocket && loadWebsocket)
                {
                    e.Request.ContinueAsync();
                }
                else if (e.Request.ResourceType == ResourceType.Other && loadOther)
                {
                    e.Request.ContinueAsync();
                }
                else
                {
                    e.Request.AbortAsync();
                }
            };

            data.Logger.Log($"Request interception done", LogColors.DarkSalmon);
        }

        private static PuppeteerSharp.Page GetPage(BotData data) => (PuppeteerSharp.Page)data.Objects["puppeteerPage"] ?? throw new Exception("No pages open!");

        private static void SwitchToMainFramePrivate(BotData data) => data.Objects["puppeteerFrame"] = GetPage(data).MainFrame;
    }
}
