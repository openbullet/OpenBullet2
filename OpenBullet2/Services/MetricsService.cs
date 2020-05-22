using System;

namespace OpenBullet2.Services
{
    public class MetricsService
    {
        private DateTime startTime = DateTime.Now;
        public TimeSpan UpTime => DateTime.Now - startTime;
    }
}
