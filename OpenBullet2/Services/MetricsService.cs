using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class MetricsService
    {
        public string OS 
        { 
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Environment.OSVersion.VersionString;

                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return $"macOS {Environment.OSVersion.Version}";

                else
                    return RuntimeInformation.OSDescription;
            }
        }

        private DateTime startTime = DateTime.Now;
        public TimeSpan UpTime => DateTime.Now - startTime;

        public string CWD => System.IO.Directory.GetCurrentDirectory();
        
        public long MemoryUsage => Process.GetCurrentProcess().WorkingSet64;

        private double cpuUsage = 0;
        public double CpuUsage => cpuUsage;

        private long netUpload = 0;
        public long NetUpload => netUpload;

        private long netDownload = 0;
        public long NetDownload => netDownload;

        public DateTime ServerTime => DateTime.Now;

        public string BuildNumber { get; private set; }
        public DateTime BuildDate { get; private set; }

        public MetricsService()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            BuildDate = new DateTime(2000, 1, 1)
                                    .AddDays(version.Build).AddSeconds(version.Revision * 2);
            BuildNumber = $"{version.Build}.{version.Revision}";
        }

        public async Task UpdateCpuUsage()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            cpuUsage = cpuUsageTotal * 100;
        }

        public async Task UpdateNetworkUsage()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return;

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var startUpload = GetCurrentNetUpload(interfaces);
            var startDownload = GetCurrentNetDownload(interfaces);

            await Task.Delay(1000);
            netUpload = GetCurrentNetUpload(interfaces) - startUpload;
            netDownload = GetCurrentNetDownload(interfaces) - startDownload;
        }

        private static long GetCurrentNetUpload(NetworkInterface[] interfaces)
        {
            try
            {
                return interfaces.Select(i => i.GetIPv4Statistics().BytesSent).Sum();
            }
            catch
            {
                return 0;
            }
        }

        private static long GetCurrentNetDownload(NetworkInterface[] interfaces)
        {
            try
            {
                return interfaces.Select(i => i.GetIPv4Statistics().BytesReceived).Sum();
            }
            catch
            {
                return 0;
            }
        }
    }
}
