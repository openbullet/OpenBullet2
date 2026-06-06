using OpenBullet2.Core.Entities;
using OpenBullet2.Native.Helpers;
using RuriLib.Functions.Files;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for AddWordlistDialog.xaml
/// </summary>
public partial class AddWordlistDialog : Page
{
    private readonly object caller;
    private readonly EnvironmentSettings env;

    public AddWordlistDialog(object caller, RuriLibSettingsService rlSettingsService)
    {
        this.caller = caller;
        InitializeComponent();

        env = rlSettingsService.Environment;

        typeCombobox.ItemsSource = env.WordlistTypes.Select(t => t.Name);
        typeCombobox.SelectedIndex = 0;
    }

    private void SearchInFolder(object sender, MouseButtonEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Wordlist files | *.txt",
            FilterIndex = 1
        };

        ofd.ShowDialog();
        locationTextbox.Text = ofd.FileName;
        nameTextbox.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

        // Set the recognized wordlist type
        try
        {
            var first = File.ReadLines(ofd.FileName).First();
            typeCombobox.Text = env.RecognizeWordlistType(first);
        }
        catch { }
    }

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nameTextbox.Text))
            {
                Alert.Error("Invalid name", "The name cannot be blank");
                return;
            }

            var path = locationTextbox.Text;
            var cwd = Directory.GetCurrentDirectory();

            // Make the path relative if inside the CWD
            if (path.StartsWith(cwd))
            {
                path = path[(cwd.Length + 1)..];
            }

            var entity = new WordlistEntity
            {
                Name = nameTextbox.Text,
                FileName = path.Replace("\\", "/"),
                Type = typeCombobox.Text,
                Purpose = purposeTextbox.Text,
                Total = FileUtils.CountLines(path)
            };

            if (caller is Wordlists page)
            {
                await page.AddWordlistAsync(entity);
            }
            else if (caller is MultiRunJobOptionsDialog dialog)
            {
                await dialog.AddWordlistAsync(entity);
            }

            ((MainDialog)Parent).Close();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
}
