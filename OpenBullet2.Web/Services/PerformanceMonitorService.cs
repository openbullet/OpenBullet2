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
    private long? _previousSampleTimestamp;
    private TimeSpan? _previousCpuTime;
    private long? _previousUploadBytes;
    private long? _previousDownloadBytes;

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
            string[] connectionIds;

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                connectionIds = [.. _connections];
            }
            finally
            {
                _semaphore.Release();
            }

            if (connectionIds.Length == 0)
            {
                ResetSampleState();
                continue;
            }

            var metrics = ReadMetrics();
            await _hub.Clients.Clients(connectionIds).SendAsync(
                SystemPerformanceMethods.NewMetrics, metrics, cancellationToken);
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }

    private PerformanceMetrics ReadMetrics()
    {
        var sampleTimestamp = Stopwatch.GetTimestamp();
        _currentProcess.Refresh();

        var memory = _currentProcess.PrivateMemorySize64;
        var cpuTime = _currentProcess.TotalProcessorTime;
        var (uploadBytes, downloadBytes) = ReadCurrentNetworkTotals();
        var cpuUsage = 0d;
        var networkUpload = 0L;
        var networkDownload = 0L;

        if (_previousSampleTimestamp is long previousTimestamp
            && _previousCpuTime is TimeSpan previousCpuTime
            && _previousUploadBytes is long previousUploadBytes
            && _previousDownloadBytes is long previousDownloadBytes)
        {
            var elapsed = Stopwatch.GetElapsedTime(previousTimestamp, sampleTimestamp);

            if (elapsed > TimeSpan.Zero)
            {
                var cpuUsedMs = (cpuTime - previousCpuTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * elapsed.TotalMilliseconds);
                cpuUsage = Math.Round(cpuUsageTotal * 100, 2);

                networkUpload = CalculateBytesPerSecond(previousUploadBytes, uploadBytes, elapsed);
                networkDownload = CalculateBytesPerSecond(previousDownloadBytes, downloadBytes, elapsed);
            }
        }

        _previousSampleTimestamp = sampleTimestamp;
        _previousCpuTime = cpuTime;
        _previousUploadBytes = uploadBytes;
        _previousDownloadBytes = downloadBytes;

        return new PerformanceMetrics
        {
            MemoryUsage = memory,
            CpuUsage = cpuUsage,
            NetworkDownload = networkDownload,
            NetworkUpload = networkUpload,
            Time = DateTime.UtcNow
        };
    }

    private void ResetSampleState()
    {
        _previousSampleTimestamp = null;
        _previousCpuTime = null;
        _previousUploadBytes = null;
        _previousDownloadBytes = null;
    }

    private static long CalculateBytesPerSecond(long previousBytes, long currentBytes, TimeSpan elapsed)
    {
        if (elapsed <= TimeSpan.Zero || currentBytes < previousBytes)
        {
            return 0;
        }

        return (long)Math.Round((currentBytes - previousBytes) / elapsed.TotalSeconds);
    }

    private static (long upload, long download) ReadCurrentNetworkTotals()
    {
        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (0, 0);
            }

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            long upload = 0;
            long download = 0;

            foreach (var netInterface in interfaces)
            {
                var stats = netInterface.GetIPv4Statistics();
                upload += stats.BytesSent;
                download += stats.BytesReceived;
            }

            return (upload, download);
        }
        catch
        {
            return (0, 0);
        }
    }
}

/// <summary>
/// Performance metrics for the system.
/// </summary>
public struct PerformanceMetrics
{
    /// <summary>
    /// The private memory usage for this process, in bytes.
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
