using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace Updater
{
    internal class Program
    {
        private static Version remoteVersion = new(0, 1, 0);
        private static Version currentVersion = new(0, 1, 0);
        private static JToken release = null;
        private static Stream stream;

        private static void Main(string[] args)
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri("https://api.github.com/repos/openbullet/OpenBullet2/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:84.0) Gecko/20100101 Firefox/84.0");

            // Fetch info from remote
            Console.Write("[1/6] Fetching version info from remote... ");
            try
            {
                // Query the github api to get a list of the latest releases
                var response = client.GetAsync("releases").Result;

                // Take the first and get its name
                var json = response.Content.ReadAsStringAsync().Result;
                release = JToken.Parse(json)[0];
                var releaseName = release["name"].ToString();

                // Try to parse that name to a Version object
                remoteVersion = Version.Parse(releaseName);
                Console.WriteLine($"Found version {remoteVersion}");
            }
            catch (Exception ex)
            {
                ExitWithError(ex);
            }

            // Read version.txt to get the current version
            Console.Write("[2/6] Reading the current version... ");
            try
            {
                var content = File.ReadLines("version.txt").First();
                currentVersion = Version.Parse(content);
                Console.WriteLine($"Found version {currentVersion}");
            }
            catch
            {
                Console.WriteLine($"Failed! Assuming version is {currentVersion}");
            }

            // Compare versions
            Console.Write("[3/6] Comparing versions... ");
            if (remoteVersion > currentVersion)
            {
                Console.WriteLine("Update available!");
            }
            else
            {
                Console.WriteLine("Already up to date!");
                Environment.Exit(0);
            }

            // Download the remote patch (not the entire build)
            var patch = release["assets"].First(t => t["name"].ToObject<string>().Contains("patch", StringComparison.OrdinalIgnoreCase));
            var size = patch["size"].ToObject<double>();
            var megaBytes = size / (1 * 1000 * 1000);
            Console.Write($"[4/6] Downloading the updated build ({megaBytes:0.00} MB)... ");
            try
            {
                var downloadUrl = patch["browser_download_url"].ToString();
                var response = client.GetAsync(downloadUrl).Result;
                stream = response.Content.ReadAsStream();
                stream.Seek(0, SeekOrigin.Begin);
                
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                ExitWithError(ex);
            }

            // Extract it
            Console.Write("[5/6] Extracting the archive... ");
            try
            {
                using var archive = new ZipArchive(stream);
                foreach (var entry in archive.Entries)
                {
                    // Some entries may fail to extract, for example the Updater because
                    // it's currently being executed, so it's better to just ignore them
                    try
                    {
                        entry.ExtractToFile(Path.Combine(Directory.GetCurrentDirectory(), entry.FullName), true);
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                ExitWithError(ex);
            }

            // Write the new version
            Console.Write("[6/6] Changing the current version number... ");
            try
            {
                File.WriteAllText("version.txt", remoteVersion.ToString());
                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                ExitWithError(ex);
            }

            Console.WriteLine("The update was completed successfully. You may now restart your OpenBullet 2 instance!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        private static void ExitWithError(Exception ex)
        {
            Console.WriteLine($"Failed! {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
