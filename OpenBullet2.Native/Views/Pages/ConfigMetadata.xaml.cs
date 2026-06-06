using Microsoft.Win32;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for ConfigMetadata.xaml
/// </summary>
public partial class ConfigMetadata : Page
{
    private readonly ConfigMetadataViewModel vm;

    public ConfigMetadata(ConfigMetadataViewModel vm)
    {
        this.vm = vm;
        DataContext = vm;

        InitializeComponent();
    }

    public void UpdateViewModel() => vm.UpdateViewModel();

    private void OpenIcon(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Images | *.ico;*.jpg;*.jpeg;*.png;*.bmp",
            FilterIndex = 1
        };

        ofd.ShowDialog();

        if (!string.IsNullOrEmpty(ofd.FileName))
        {
            try
            {
                vm.SetIconFromFile(ofd.FileName);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }

    private async void DownloadIcon(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.SetIconFromUrlAsync(urlTextbox.Text);
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void PasteIcon(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Clipboard.ContainsImage())
            {
                throw new InvalidOperationException("The clipboard does not contain an image");
            }

            var image = Clipboard.GetImage()
                ?? throw new InvalidOperationException("Failed to read the image from the clipboard");

            vm.SetIconFromClipboard(image);
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private async void PageKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            try
            {
                await vm.Save();
                Alert.ToastSuccess("Saved", $"{vm.Name} was saved successfully!");
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }
}
