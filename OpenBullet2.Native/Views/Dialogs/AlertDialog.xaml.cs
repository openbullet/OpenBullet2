using MahApps.Metro.IconPacks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AlertDialog.xaml
    /// </summary>
    public partial class AlertDialog : Page
    {
        public AlertDialog(AlertType type, string title, string message)
        {
            InitializeComponent();

            this.title.Text = title;
            this.message.Text = message;

            icon.Kind = type switch
            {
                AlertType.Success => PackIconOcticonsKind.Check,
                AlertType.Warning => PackIconOcticonsKind.Alert,
                AlertType.Error => PackIconOcticonsKind.X,
                AlertType.Info => PackIconOcticonsKind.Info,
                _ => throw new NotImplementedException()
            };

            icon.Foreground = type switch
            {
                AlertType.Success => Brushes.YellowGreen,
                AlertType.Warning => Brushes.Orange,
                AlertType.Error => Brushes.Tomato,
                AlertType.Info => Brushes.SkyBlue,
                _ => throw new NotImplementedException()
            };

            okButton.Focus();
        }

        private void Ok(object sender, RoutedEventArgs e) => ((MainDialog)Parent).Close();

        private void PageKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((MainDialog)Parent).Close();
            }
        }
    }

    public enum AlertType
    {
        Success,
        Warning,
        Error,
        Info
    }
}
