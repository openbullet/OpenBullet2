using OpenBullet2.Core.Services;
using OpenBullet2.Native.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for ChangeBotsDialog.xaml
/// </summary>
public partial class ChangeBotsDialog : Page
{
    private readonly object caller;

    public ChangeBotsDialog(object caller, int oldValue, JobFactoryService jobFactoryService)
    {
        this.caller = caller;

        InitializeComponent();
        bots.Maximum = jobFactoryService.BotLimit;
        bots.Value = oldValue;
    }

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            if (bots.Value is not double value)
            {
                return;
            }

            if (caller is MultiRunJobViewer mr)
            {
                await mr.ChangeBotsAsync((int)value);
            }
            else if (caller is ProxyCheckJobViewer pc)
            {
                await pc.ChangeBotsAsync((int)value);
            }

            ((MainDialog)Parent).Close();
        }
        catch (Exception ex)
        {
            Helpers.Alert.Exception(ex);
        }
    }
}
