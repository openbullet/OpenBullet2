using OpenBullet2.Native.DTOs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Views.Pages;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for DeleteLowQualityProxiesDialog.xaml
/// </summary>
public partial class DeleteLowQualityProxiesDialog : Page
{
    private readonly object caller;

    public DeleteLowQualityProxiesDialog(object caller)
    {
        this.caller = caller;
        InitializeComponent();
    }

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            var dto = new DeleteLowQualityProxiesDto
            {
                DeleteUnknown = deleteUnknownCheckbox.IsChecked == true,
                DeleteTransparent = deleteTransparentCheckbox.IsChecked == true,
                DeleteAnonymous = deleteAnonymousCheckbox.IsChecked == true
            };

            if (!dto.DeleteUnknown && !dto.DeleteTransparent && !dto.DeleteAnonymous)
            {
                Alert.Error("Invalid selection", "Select at least one quality to delete.");
                return;
            }

            if (caller is Proxies page)
            {
                await page.DeleteLowQualityAsync(dto);
            }

            await Dispatcher.InvokeAsync(() => ((MainDialog)Parent).Close());
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
}
