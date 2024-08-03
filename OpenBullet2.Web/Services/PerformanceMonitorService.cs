using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Web.Dtos.Info;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.SignalR;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Service that monitors the system's performance.
/// </summary>
public class PerformanceMonitorService : IHostedService
{
    private readonly List<string> _connections = new();
    private readonly IHubContext<SystemPerformanceHub> _hub;
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1);
    private CancellationTokenSource _cts = new();

    /// <summary></summary>
    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger,
        IHubContext<SystemPerformanceHub> hub)
    {
        _hub = hub;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ReadMetricsLoopAsync().Forget(e =>
        {
            // Don't log OperationCanceledException
            if (e is OperationCanceledException)
            {
                return;
            }
            
            _logger.LogError(new EventId(0), e, "Got an error while reading performance metrics");
        });

        // Dispose the old cancellation token source and create a new one
        _cts.Dispose();
        _cts = new CancellationTokenSource();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers a new connection.
    /// </summary>
    public async Task RegisterConnectionAsync(string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            _connections.Add(connectionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Unregisters an existing connection.
    /// </summary>
    public async Task UnregisterConnectionAsync(string connectionId)
    {
        await _semaphore.WaitAsync();

        try
        {
            _connections.Remove(connectionId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ReadMetricsLoopAsync()
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        do
        {
            var memory = Process.GetCurrentProcess().WorkingSet64;
            var cpu = await ReadCpuUsageAsync(_cts.Token);
            var (upload, download) = await ReadNetworkUsage();

            var metrics = new PerformanceMetrics {
                MemoryUsage = memory,
                CpuUsage = cpu,
                NetworkDownload = download,
                NetworkUpload = upload,
                Time = DateTime.UtcNow
            };

            // Send the metrics to all connected clients
            await _semaphore.WaitAsync();

            try
            {
                await _hub.Clients.Clients(_connections).SendAsync(
                    SystemPerformanceMethods.NewMetrics, metrics);
            }
            finally
            {
                _semaphore.Release();
            }
        } while (await timer.WaitForNextTickAsync(_cts.Token));
    }

    private static async Task<double> ReadCpuUsageAsync(CancellationToken cancellationToken)
    {
        var sw = new Stopwatch();

        sw.Start();
        var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        await Task.Delay(100, cancellationToken);

        sw.Stop();
        var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * sw.ElapsedMilliseconds);

        return Math.Round(cpuUsageTotal * 100, 2);
    }

    private static async Task<(long upload, long download)> ReadNetworkUsage()
    {
        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (0, 0);
            }
            
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var startUpload = GetCurrentNetUpload(interfaces);
            var startDownload = GetCurrentNetDownload(interfaces);

            await Task.Delay(100);

            var netUpload = GetCurrentNetUpload(interfaces) - startUpload;
            var netDownload = GetCurrentNetDownload(interfaces) - startDownload;
            return (netUpload * 10, netDownload * 10);    
        }
        catch
        {
            return (0, 0);
        }
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

/// <summary>
/// Performance metrics for the system.
/// </summary>
public struct PerformanceMetrics
{
    /// <summary>
    /// The memory usage for this process, in bytes.
    /// </summary>
    public long MemoryUsage { get; set; }

    /// <summary>
    /// The CPU usage for this process, as a percentage.
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// The network upload bandwidth for the whole system, in bytes.
    /// </summary>
    public long NetworkUpload { get; set; }

    /// <summary>
    /// The network download bandwitdh for the whole system, in bytes.
    /// </summary>
    public long NetworkDownload { get; set; }

    /// <summary>
    /// The time at which the metrics were read.
    /// </summary>
    public DateTime Time { get; set; }
}
