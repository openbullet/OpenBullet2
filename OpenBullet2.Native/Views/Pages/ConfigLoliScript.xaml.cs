using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using RuriLib.Models.Configs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for ConfigLoliScript.xaml
/// </summary>
public partial class ConfigLoliScript : Page
{
    private readonly MainWindow mainWindow;
    private readonly ConfigService configService;

    public ConfigLoliScript(MainWindow mainWindow, ConfigService configService)
    {
        this.mainWindow = mainWindow;
        this.configService = configService;
        InitializeComponent();

        HighlightSyntax();
    }

    public void UpdateViewModel()
    {
        try
        {
            if (configService.SelectedConfig.Mode != ConfigMode.Legacy)
            {
                throw new Exception("This page is only available for legacy configs");
            }

            editor.Text = configService.SelectedConfig.LoliScript;
        }
        catch (Exception ex)
        {
            // On fail, prompt it to the user and go back to the configs page
            Alert.Exception(ex);
            mainWindow.NavigateTo(MainWindowPage.Configs);
        }
    }

    /// <summary>
    /// Call this when changing page via the dropdown menu otherwise it
    /// will not trigger the LostFocus event on the editor.
    /// </summary>
    public void OnPageChanged() => configService.SelectedConfig.LoliScript = editor.Text;

    private void EditorLostFocus(object sender, RoutedEventArgs e)
        => configService.SelectedConfig.LoliScript = editor.Text;

    private void HighlightSyntax()
    {
        using var reader = XmlReader.Create("Highlighting/LoliScript.xshd");
        editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
        editor.TextArea.TextView.LinkTextUnderline = false;
    }

    private async void PageKeyDown(object sender, KeyEventArgs e)
    {
        // Save on CTRL+S
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            configService.SelectedConfig.LoliScript = editor.Text;
            await configService.SaveSelectedConfigAsync();
            Alert.ToastSuccess("Saved", $"{configService.SelectedConfig.Metadata.Name} was saved successfully!");
        }
    }
}
