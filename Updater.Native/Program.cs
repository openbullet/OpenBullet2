using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace Updater.Native
{
    internal class Program
    {
        private static Version remoteVersion = new(0, 1, 0);
        private static Version currentVersion = new(0, 1, 0);
        private static JToken release = null;
        private static Stream stream;

        private static void Main(string[] args)
        {
            Console.Title = "OpenBullet 2 - Native Client Updater";

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
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Download the entire remote client
            // TODO: If it's too big, use the patch system like for the web UI
            var patch = release["assets"].First(t => t["name"].ToObject<string>().Contains("OpenBullet2.Native", StringComparison.OrdinalIgnoreCase));
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
                        var folder = Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(entry.FullName));
                        Directory.CreateDirectory(folder);
                        entry.ExtractToFile(Path.Combine(Directory.GetCurrentDirectory(), entry.FullName), true);
                    }
                    catch
                    {

                    }
                }

                Console.WriteLine("Done!");
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

            Console.WriteLine("The update was completed successfully. Restarting the client...");
            Thread.Sleep(1000);

            // Reopen the native client
            Process.Start("OpenBullet2.Native.exe");

            Environment.Exit(0);
        }

        private static void ExitWithError(Exception ex)
        {
            Console.WriteLine($"Failed! {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
