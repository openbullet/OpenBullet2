using OpenBullet2.Native.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
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
            Process.Start("Updater.Native.exe");
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
}
