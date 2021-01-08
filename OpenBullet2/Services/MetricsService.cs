using System;
using System.Diagnostics;
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
                    return $"OSX {Environment.OSVersion.Version}";

                else
                    return RuntimeInformation.OSDescription;
            }
        }

        private DateTime startTime = DateTime.Now;
        public TimeSpan UpTime => DateTime.Now - startTime;

        public string CWD => System.IO.Directory.GetCurrentDirectory();
        
        public long MemoryUsage => Process.GetCurrentProcess().WorkingSet64;

        public double cpuUsage = 0;
        public double CpuUsage => cpuUsage;

        public DateTime ServerTime => DateTime.Now;

        public string BuildNumber { get; private set; }
        public DateTime BuildDate { get; private set; }

        public MetricsService()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
    }
}
