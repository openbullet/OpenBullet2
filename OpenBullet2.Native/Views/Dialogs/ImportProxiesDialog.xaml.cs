using Microsoft.Win32;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Models.Proxies;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportProxiesDialog.xaml
    /// </summary>
    public partial class ImportProxiesDialog : Page
    {
        private readonly object caller;

        public ImportProxiesDialog(object caller)
        {
            this.caller = caller;
            InitializeComponent();

            proxyTypeCombobox.ItemsSource = Enum.GetNames(typeof(ProxyType));
            proxyTypeCombobox.SelectedIndex = 0;
        }

        private void SearchInFolder(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Proxy files | *.txt",
                FilterIndex = 1
            };

            ofd.ShowDialog();
            locationTextbox.Text = ofd.FileName;
        }

        private async void Accept(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (modeTabControl.SelectedIndex)
                {
                    // File
                    case 0:
                        ReturnLines(await File.ReadAllTextAsync(locationTextbox.Text).ConfigureAwait(false));
                        break;

                    // Paste
                    case 1:
                        ReturnLines(proxiesBox.Text);
                        break;

                    // Remote
                    case 2:
                        await ImportFromUrl(urlTextbox.Text);
                        break;
                }
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private async Task ImportFromUrl(string url)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage();

            request.RequestUri = new Uri(url);
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

            using var response = await client.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();
            ReturnLines(text);
        }

        private void ReturnLines(string text)
        {
            var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var dto = new DTOs.ProxiesForImportDto
            {
                Lines = lines,
                DefaultType = proxyTypeCombobox.SelectedItem.AsEnum<ProxyType>(),
                DefaultUsername = usernameTextbox.Text,
                DefaultPassword = passwordTextbox.Text
            };

            if (caller is Proxies page)
            {
                page.AddProxies(dto);
            }
            ((MainDialog)Parent).Close();
        }

        private void SelectFileMode(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Brush.Get("ForegroundMenuSelected");
            pasteMode.Foreground = Brush.Get("ForegroundMain");
            remoteMode.Foreground = Brush.Get("ForegroundMain");
            modeTabControl.SelectedIndex = 0;
        }

        private void SelectPasteMode(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Brush.Get("ForegroundMain");
            pasteMode.Foreground = Brush.Get("ForegroundMenuSelected");
            remoteMode.Foreground = Brush.Get("ForegroundMain");
            modeTabControl.SelectedIndex = 1;
        }

        private void SelectRemoteMode(object sender, MouseButtonEventArgs e)
        {
            fileMode.Foreground = Brush.Get("ForegroundMain");
            pasteMode.Foreground = Brush.Get("ForegroundMain");
            remoteMode.Foreground = Brush.Get("ForegroundMenuSelected");
            modeTabControl.SelectedIndex = 2;
        }
    }
}
