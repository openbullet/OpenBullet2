using OpenBullet2.Native.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for UpdateConfirmationDialog.xaml
/// </summary>
public partial class UpdateConfirmationDialog : Page
{
    private readonly UpdateConfirmationDialogViewModel vm;

    public UpdateConfirmationDialog(Version current, Version remote)
    {
        InitializeComponent();

        vm = new UpdateConfirmationDialogViewModel(current, remote);
        DataContext = vm;
    }

    private void Confirm(object sender, RoutedEventArgs e)
    {
        var updaterFileName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.Arm64 => "ob2-native-updater-win-arm64.exe",
            Architecture.X64 => "ob2-native-updater-win-x64.exe",
            _ => throw new PlatformNotSupportedException("Native updates are only published for x64 and arm64")
        };

        var installDirectory = AppContext.BaseDirectory;
        var startInfo = new ProcessStartInfo(Path.Combine(installDirectory, updaterFileName))
        {
            WorkingDirectory = installDirectory
        };

        Process.Start(startInfo);
        Environment.Exit(0);
    }

    private void GoBack(object sender, RoutedEventArgs e) => ((MainDialog)Parent).Close();
}

public class UpdateConfirmationDialogViewModel : ViewModelBase
{
    private string currentVersion = string.Empty;
    public string CurrentVersion
    {
        get => currentVersion;
        set
        {
            currentVersion = value;
            OnPropertyChanged();
        }
    }

    private string remoteVersion = string.Empty;
    public string RemoteVersion
    {
        get => remoteVersion;
        set
        {
            remoteVersion = value;
            OnPropertyChanged();
        }
    }

    public UpdateConfirmationDialogViewModel(Version current, Version remote)
    {
        CurrentVersion = current.ToString();
        RemoteVersion = remote.ToString();
    }
}
