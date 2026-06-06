using Microsoft.Win32;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for OBSettings.xaml
/// </summary>
public partial class OBSettings : Page
{
    private readonly OBSettingsViewModel vm;

    public OBSettings(OBSettingsViewModel vm)
    {
        this.vm = vm;
        DataContext = vm;

        InitializeComponent();

        configSectionOnLoadCombobox.ItemsSource = Enum.GetValues(typeof(ConfigSection)).Cast<ConfigSection>();
        updateChannelCombobox.ItemsSource = Enum.GetValues(typeof(UpdateChannel)).Cast<UpdateChannel>();
    }

    private async void Save(object sender, RoutedEventArgs e)
    {
        try
        {
            await vm.Save();
            Alert.ToastSuccess("Saved", "OB settings were saved successfully!");
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
    private void Reset(object sender, RoutedEventArgs e) => vm.Reset();
    private void ResetCustomization(object sender, RoutedEventArgs e) => vm.ResetCustomization();

    private void AddProxyCheckTarget(object sender, RoutedEventArgs e) => vm.AddProxyCheckTarget();
    private void RemoveProxyCheckTarget(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ProxyCheckTarget target })
        {
            vm.RemoveProxyCheckTarget(target);
        }
    }

    private void AddCustomSnippet(object sender, RoutedEventArgs e) => vm.AddCustomSnippet();
    private void RemoveCustomSnippet(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: CustomSnippet snippet })
        {
            vm.RemoveCustomSnippet(snippet);
        }
    }

    private void AddRemoteConfigsEndpoint(object sender, RoutedEventArgs e) => vm.AddRemoteConfigsEndpoint();
    private void RemoveRemoteConfigsEndpoint(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: RemoteConfigsEndpoint endpoint })
        {
            vm.RemoveRemoteConfigsEndpoint(endpoint);
        }
    }

    private void ChooseBackgroundImage(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Images | *.jpg;*.jpeg;*.png;*.bmp",
            FilterIndex = 1
        };

        ofd.ShowDialog();

        if (!string.IsNullOrEmpty(ofd.FileName))
        {
            try
            {
                vm.SetBackgroundImage(ofd.FileName);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }
}
