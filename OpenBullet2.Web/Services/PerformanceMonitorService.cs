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
public sealed class PerformanceMonitorService : IHostedService, IDisposable
{
    private readonly List<string> _connections = new();
    private readonly Process _currentProcess = Process.GetCurrentProcess();
    private readonly IHubContext<SystemPerformanceHub> _hub;
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1);
    private CancellationTokenSource _cts = new();
    private bool _disposed;

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
        // Dispose the old cancellation token source and create a new one
        _cts.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        ReadMetricsLoopAsync(_cts.Token).Forget(e =>
        {
            // Don't log OperationCanceledException
            if (e is OperationCanceledException)
            {
                return;
            }

            _logger.LogError(new EventId(0), e, "Got an error while reading performance metrics");
        });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing)
        {
            return;
        }

        _cts.Dispose();
        _semaphore.Dispose();
        _currentProcess.Dispose();
        _disposed = true;
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

    private async Task ReadMetricsLoopAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        do
        {
            _currentProcess.Refresh();
            var memory = _currentProcess.WorkingSet64;
            var cpu = await ReadCpuUsageAsync(_currentProcess, cancellationToken);
            var (upload, download) = await ReadNetworkUsage(cancellationToken);

            var metrics = new PerformanceMetrics
            {
                MemoryUsage = memory,
                CpuUsage = cpu,
                NetworkDownload = download,
                NetworkUpload = upload,
                Time = DateTime.UtcNow
            };

            // Send the metrics to all connected clients
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                await _hub.Clients.Clients(_connections).SendAsync(
                    SystemPerformanceMethods.NewMetrics, metrics, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    private static async Task<double> ReadCpuUsageAsync(Process currentProcess, CancellationToken cancellationToken)
    {
        var sw = new Stopwatch();

        sw.Start();
        currentProcess.Refresh();
        var startCpuUsage = currentProcess.TotalProcessorTime;

        await Task.Delay(100, cancellationToken);

        sw.Stop();
        currentProcess.Refresh();
        var endCpuUsage = currentProcess.TotalProcessorTime;

        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * sw.ElapsedMilliseconds);

        return Math.Round(cpuUsageTotal * 100, 2);
    }

    private static async Task<(long upload, long download)> ReadNetworkUsage(CancellationToken cancellationToken)
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

            await Task.Delay(100, cancellationToken);

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
