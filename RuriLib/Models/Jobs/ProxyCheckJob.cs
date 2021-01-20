using RuriLib.Functions.Http;
using RuriLib.Logging;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using RuriLib.Threading;
using RuriLib.Threading.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs
{
    public class ProxyCheckJob : Job
    {
        // Options
        public int Bots { get; set; } = 1;
        public bool CheckOnlyUntested { get; set; } = true;
        public string Url { get; set; } = "https://google.com";
        public string SuccessKey { get; set; } = "title>Google";
        public IEnumerable<Proxy> Proxies { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
        public IProxyCheckOutput ProxyOutput { get; set; }
        public IProxyGeolocationProvider GeoProvider { get; set; }

        public Parallelizer<ProxyCheckerInput, Proxy> Parallelizer { get; private set; }

        // Stats
        public int Total { get; set; }
        public int Tested { get; set; }
        public int Working { get; set; }
        public int NotWorking { get; set; }

        public ProxyCheckJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
            
        }

        private Func<ProxyCheckerInput, CancellationToken, Task<Proxy>> workFunction = 
            new Func<ProxyCheckerInput, CancellationToken, Task<Proxy>>(async (input, token) =>
        {
            var options = new HttpHandlerOptions
            {
                Cookies = new CookieContainer(),
                Timeout = input.Timeout
            };

            using var handler = HttpHandlerFactory.GetHandler(input.Proxy, options);
            using var http = new HttpClient(handler) { Timeout = input.Timeout };
            
            try
            {
                // Use 2 cancellation tokens since we need to control the proxy connect timeout as well
                var cts = new CancellationTokenSource();
                cts.CancelAfter(input.Timeout);

                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

                var sw = new Stopwatch();
                sw.Start();
                var response = await http.GetAsync(input.Url, linkedCts.Token);
                var content = await response.Content.ReadAsStringAsync();
                sw.Stop();

                if (content.Contains(input.SuccessKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    input.Proxy.WorkingStatus = ProxyWorkingStatus.Working;
                    input.Proxy.Ping = (int)sw.ElapsedMilliseconds;
                }
                else
                {
                    input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                    input.Proxy.Ping = 0;
                }
            }
            catch (OperationCanceledException)
            {
                input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                input.Proxy.Ping = (int)input.Timeout.TotalMilliseconds;
            }
            catch
            {
                input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                input.Proxy.Ping = 0;
            }
            finally
            {
                input.Proxy.LastChecked = DateTime.Now;
                http.Dispose();
            }

            // Geolocation
            if (input.GeoProvider != null)
            {
                try
                {
                    input.Proxy.Country = await input.GeoProvider.Geolocate(input.Proxy.Host);
                }
                catch
                {
                    input.Proxy.Country = "Unknown";
                }
            }

            return input.Proxy;
        });

        public override async Task Start()
        {
            if (Proxies == null)
                throw new NullReferenceException("The proxy list cannot be null");

            if (ProxyOutput == null)
                throw new NullReferenceException("The proxy check output cannot be null");

            if (Url == null)
                throw new NullReferenceException("The url cannot be null");

            if (SuccessKey == null)
                throw new NullReferenceException("The success key cannot be null");

            // Wait for the start condition to be verified
            // await base.Start();

            var proxies = CheckOnlyUntested 
                ? Proxies.Where(p => p.WorkingStatus == ProxyWorkingStatus.Untested)
                : Proxies;
            
            var workItems = proxies.Select(p => new ProxyCheckerInput(p, Url, SuccessKey, Timeout, GeoProvider));
            Parallelizer = ParallelizerFactory<ProxyCheckerInput, Proxy>
                .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems, workFunction, Bots, Proxies.Count(), 0);
            Parallelizer.NewResult += UpdateProxy;
            Parallelizer.StatusChanged += StatusChanged;

            ServicePointManager.DefaultConnectionLimit = 200;

            await Parallelizer.Start();
        }

        public override async Task Stop()
        {
            await Parallelizer?.Stop();
        }

        public override async Task Abort()
        {
            await Parallelizer?.Abort();
        }

        public override async Task Pause()
        {
            await Parallelizer?.Pause();
        }

        public override async Task Resume()
        {
            await Parallelizer?.Resume();
        }

        private void StatusChanged(object sender, ParallelizerStatus status)
        {
            Status = status switch
            {
                ParallelizerStatus.Idle => JobStatus.Idle,
                ParallelizerStatus.Starting => JobStatus.Starting,
                ParallelizerStatus.Running => JobStatus.Running,
                ParallelizerStatus.Pausing => JobStatus.Pausing,
                ParallelizerStatus.Paused => JobStatus.Paused,
                ParallelizerStatus.Stopping => JobStatus.Stopping,
                ParallelizerStatus.Resuming => JobStatus.Resuming,
                _ => throw new NotImplementedException()
            };
        }

        private void UpdateProxy(object sender, ResultDetails<ProxyCheckerInput, Proxy> details)
        {
            var proxy = details.Result;

            if (proxy.WorkingStatus == ProxyWorkingStatus.Working) Working++;
            else NotWorking++;

            Tested++;

            // This is fire and forget
            ProxyOutput.Store(proxy);
        }
    }

    public struct ProxyCheckerInput
    {
        public Proxy Proxy { get; set; }
        public string Url { get; set; }
        public string SuccessKey { get; set; }
        public TimeSpan Timeout { get; set; }
        public IProxyGeolocationProvider GeoProvider { get; set; }

        public ProxyCheckerInput(Proxy proxy, string url, string successKey,
            TimeSpan timeout, IProxyGeolocationProvider geoProvider)
        {
            Proxy = proxy;
            Url = url;
            SuccessKey = successKey;
            Timeout = timeout;
            GeoProvider = geoProvider;
        }
    }
}
