using OpenBullet2.Core.Services;
using OpenBullet2.Native.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ChangeBotsDialog.xaml
    /// </summary>
    public partial class ChangeBotsDialog : Page
    {
        private readonly object caller;

        public ChangeBotsDialog(object caller, int oldValue)
        {
            this.caller = caller;

            InitializeComponent();
            bots.Maximum = SP.GetService<JobFactoryService>().BotLimit;
            bots.Value = oldValue;
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            if (caller is MultiRunJobViewer page)
            {
                page.ChangeBots((int)bots.Value);
            }

            ((MainDialog)Parent).Close();
        }
    }
}
