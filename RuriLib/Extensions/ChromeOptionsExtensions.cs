using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using OpenQA.Selenium.Chrome;
using RuriLib.Models.Proxies;

namespace RuriLib.Extensions;

/// <summary>
/// Extensions for ChromeOptions (Selenium Driver).
/// Credits: https://github.com/RDavydenko/OpenQA.Selenium.Chrome.ChromeDriverExtensions
/// </summary>
public static class ChromeOptionsExtensions
{
	private const string _backgroundJs = """

                                         var config = {
                                            mode: "fixed_servers",
                                             rules: {
                                                 singleProxy: {
                                                     scheme: "{PROTOCOL}",
                                                     host: "{HOST}",
                                                     port: parseInt({PORT})
                                                 },
                                                 bypassList: []
                                            }
                                         };

                                         chrome.proxy.settings.set({ value: config, scope: "regular" }, function() { });

                                         function callbackFn(details)
                                         {
                                            return {
                                             	authCredentials:
                                             	{
                                             		username: "{USERNAME}",
                                             		password: "{PASSWORD}"
                                             	}
                                            };
                                         }

                                         chrome.webRequest.onAuthRequired.addListener(
                                            callbackFn,
                                         
                                            { urls:["<all_urls>"] },
                                             ['blocking']
                                         );
                                         """;

	private const string _manifestJson = """

                                         {
                                             "version": "1.0.0",
                                             "manifest_version": 2,
                                             "name": "Chrome Proxy",
                                             "permissions": [
                                                 "proxy",
                                                 "tabs",
                                                 "unlimitedStorage",
                                                 "storage",
                                                 "<all_urls>",
                                                 "webRequest",
                                                 "webRequestBlocking"
                                             ],
                                             "background": {
                                                 "scripts": ["background.js"]
                                            },
                                             "minimum_chrome_version":"22.0.0"
                                         }
                                         """;

    /// <summary>
    /// Adds a proxy to the ChromeOptions.
    /// </summary>
    public static void AddProxy(this ChromeOptions options, Proxy proxy)
    {
        if (proxy.NeedsAuthentication)
        {
            AddAuthenticatedProxy(options, proxy);
        }
        else
        {
            options.AddArgument($"--proxy-server={proxy.Protocol}://{proxy.Host}:{proxy.Port}");
        }
    }
    
	private static void AddAuthenticatedProxy(ChromeOptions options, Proxy proxy)
	{
        if (!proxy.NeedsAuthentication)
        {
            throw new ArgumentException(
                "The proxy does not require authentication. Use the AddProxy method instead.");
        }
        
		var backgroundProxyJs = ReplaceTemplates(_backgroundJs, proxy);
        var baseDir = Path.Combine(Path.GetTempPath(), "AuthProxy", Guid.NewGuid().ToString());
        var manifestPath = Path.Combine(baseDir, "manifest.json");
        var backgroundPath = Path.Combine(baseDir, "background.js");
        var archiveFilePath = Path.Combine(baseDir, "extension.zip");
        
        // If the extension.zip file already exists, use the existing one
        if (File.Exists(archiveFilePath))
        {
            options.AddExtension(archiveFilePath);
            return;
        }
        
        if (!Directory.Exists(baseDir))
        {
            Directory.CreateDirectory(baseDir);
        }

		File.WriteAllText(manifestPath, _manifestJson);
		File.WriteAllText(backgroundPath, backgroundProxyJs);

		using (var zip = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create))
		{
			zip.CreateEntryFromFile(manifestPath, "manifest.json");
			zip.CreateEntryFromFile(backgroundPath, "background.js");
		}

		File.Delete(manifestPath);
		File.Delete(backgroundPath);

		options.AddExtension(archiveFilePath);
	}

	private static string ReplaceTemplates(string str, Proxy proxy) =>
        new StringBuilder(str)
            .Replace("{PROTOCOL}", proxy.Protocol)
            .Replace("{HOST}", proxy.Host)
            .Replace("{PORT}", proxy.Port.ToString())
            .Replace("{USERNAME}", proxy.Username)
            .Replace("{PASSWORD}", proxy.Password)
            .ToString();
}
