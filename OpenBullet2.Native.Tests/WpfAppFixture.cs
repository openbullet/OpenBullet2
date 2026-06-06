using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace OpenBullet2.Native.Tests;

public sealed class WpfAppFixture : IDisposable
{
    private const string NativeTestModeEnvironmentVariable = "OB2_NATIVE_TEST_MODE";
    private const string NativeUserDataFolderEnvironmentVariable = "Settings__UserDataFolder";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan InvocationTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);
    private readonly Thread thread;
    private readonly Dispatcher dispatcher;
    private readonly string? previousNativeTestModeValue;
    private readonly string? previousUserDataFolderValue;
    private readonly string userDataFolder;

    public WpfAppFixture()
    {
        Alert.SuppressDialogs = true;
        previousNativeTestModeValue = Environment.GetEnvironmentVariable(NativeTestModeEnvironmentVariable);
        previousUserDataFolderValue = Environment.GetEnvironmentVariable(NativeUserDataFolderEnvironmentVariable);
        userDataFolder = Path.Combine(Path.GetTempPath(), $"OB2_Native_UserData_{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable(NativeTestModeEnvironmentVariable, "1");
        Environment.SetEnvironmentVariable(NativeUserDataFolderEnvironmentVariable, userDataFolder);

        using var ready = new ManualResetEventSlim();
        Exception? startupException = null;
        Dispatcher? createdDispatcher = null;

        thread = new Thread(() =>
        {
            try
            {
                var app = new App();
                app.InitializeComponent();
                createdDispatcher = Dispatcher.CurrentDispatcher;
            }
            catch (Exception ex)
            {
                startupException = ex;
            }
            finally
            {
                ready.Set();
            }

            if (startupException is null)
            {
                Dispatcher.Run();
            }

            try
            {
                App.Host.Dispose();
            }
            catch
            {
            }
        })
        {
            IsBackground = true,
            Name = "OpenBullet2.Native.Tests.Wpf"
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        if (!ready.Wait(StartupTimeout))
        {
            throw new TimeoutException($"Timed out after {StartupTimeout.TotalSeconds:0} seconds while initializing the WPF test app");
        }

        if (startupException is not null)
        {
            throw new InvalidOperationException("Failed to initialize the WPF test app", startupException);
        }

        dispatcher = createdDispatcher ?? throw new InvalidOperationException("The WPF dispatcher was not created");
    }

    public async Task InvokeAsync(Action<IServiceProvider> action, [CallerMemberName] string operationName = "")
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dispatcherOperation = dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                action(App.Host.Services);
                completion.SetResult();
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }));

        try
        {
            await completion.Task.WaitAsync(InvocationTimeout);
        }
        catch (TimeoutException ex)
        {
            if (dispatcherOperation.Status is DispatcherOperationStatus.Pending)
            {
                dispatcherOperation.Abort();
            }

            throw new TimeoutException(
                $"Timed out after {InvocationTimeout.TotalSeconds:0} seconds while executing WPF test operation '{operationName}'",
                ex);
        }
    }

    public async Task<T> InvokeAsync<T>(Func<IServiceProvider, T> action, [CallerMemberName] string operationName = "")
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var dispatcherOperation = dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                completion.SetResult(action(App.Host.Services));
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }));

        try
        {
            return await completion.Task.WaitAsync(InvocationTimeout);
        }
        catch (TimeoutException ex)
        {
            if (dispatcherOperation.Status is DispatcherOperationStatus.Pending)
            {
                dispatcherOperation.Abort();
            }

            throw new TimeoutException(
                $"Timed out after {InvocationTimeout.TotalSeconds:0} seconds while executing WPF test operation '{operationName}'",
                ex);
        }
    }

    public void Dispose()
    {
        dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
        thread.Join(ShutdownTimeout);
        Environment.SetEnvironmentVariable(NativeTestModeEnvironmentVariable, previousNativeTestModeValue);
        Environment.SetEnvironmentVariable(NativeUserDataFolderEnvironmentVariable, previousUserDataFolderValue);
        Alert.SuppressDialogs = false;

        try
        {
            if (Directory.Exists(userDataFolder))
            {
                Directory.Delete(userDataFolder, true);
            }
        }
        catch
        {
        }
    }
}
