using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class MetricsService
    {
        public string OS => System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        private DateTime startTime = DateTime.Now;
        public TimeSpan UpTime => DateTime.Now - startTime;

        public string CWD => System.IO.Directory.GetCurrentDirectory();
        
        public long MemoryUsage => Process.GetCurrentProcess().WorkingSet64;

        public double cpuUsage = 0;
        public double CpuUsage => cpuUsage;

        public DateTime ServerTime => DateTime.Now;

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
