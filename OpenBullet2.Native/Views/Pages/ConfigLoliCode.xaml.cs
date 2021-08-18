using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Views.Pages.Shared;
using RuriLib.Models.Configs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigLoliCode.xaml
    /// </summary>
    public partial class ConfigLoliCode : Page
    {
        private readonly ConfigService configService;
        private readonly IConfigRepository configRepo; // TODO: This should not be here
        private Config Config => configService.SelectedConfig;

        public ConfigLoliCode()
        {
            InitializeComponent();
            configService = SP.GetService<ConfigService>();
            configRepo = SP.GetService<IConfigRepository>();

            HighlightSyntax();
        }

        public void UpdateViewModel()
        {
            try
            {
                // Try to change the mode to LoliCode and set the editor's text
                configService.SelectedConfig.ChangeMode(ConfigMode.LoliCode);
                editor.Text = configService.SelectedConfig.LoliCodeScript;
                usingsRTB.Document.Blocks.Clear();
                usingsRTB.AppendText(string.Join(Environment.NewLine, Config.Settings.ScriptSettings.CustomUsings), Colors.White);
            }
            catch (Exception ex)
            {
                // On fail, prompt it to the user and go back to the configs page
                Alert.Exception(ex);
                SP.GetService<MainWindow>().NavigateTo(MainWindowPage.Configs);
            }
        }

        private void EditorLostFocus(object sender, RoutedEventArgs e) 
            => configService.SelectedConfig.LoliCodeScript = editor.Text;

        private void HighlightSyntax()
        {
            using var reader = XmlReader.Create("Highlighting/LoliCode.xshd");
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            editor.TextArea.TextView.LinkTextUnderline = false;
        }

        private async void PageKeyDown(object sender, KeyEventArgs e)
        {
            // Save on CTRL+S
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                configService.SelectedConfig.LoliCodeScript = editor.Text;
                await configRepo.Save(configService.SelectedConfig);
                Alert.Info("Saved", $"{configService.SelectedConfig.Metadata.Name} was saved successfully!");
            }
        }

        private void ToggleUsings(object sender, RoutedEventArgs e) => usingsRTB.Visibility = 
            usingsRTB.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        private void UsingsChanged(object sender, TextChangedEventArgs e)
            => Config.Settings.ScriptSettings.CustomUsings = usingsRTB.Lines().Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }
}
