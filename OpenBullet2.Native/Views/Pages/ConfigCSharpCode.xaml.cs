using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigCSharpCode.xaml
    /// </summary>
    public partial class ConfigCSharpCode : Page
    {
        private readonly ConfigCSharpCodeViewModel vm;
        private readonly ConfigService configService;
        private readonly OpenBulletSettingsService obSettingsService;
        private Config Config => configService.SelectedConfig;

        public ConfigCSharpCode()
        {
            vm = new ConfigCSharpCodeViewModel();
            DataContext = vm;

            InitializeComponent();
            configService = SP.GetService<ConfigService>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();

            HighlightSyntax(editor);
            HighlightSyntax(startupEditor);
        }

        public void UpdateViewModel()
        {
            try
            {
                // Transpile if not in CSharp mode
                if (Config != null && Config.Mode != ConfigMode.CSharp)
                {
                    Config.CSharpScript = Config.Mode == ConfigMode.Stack
                            ? Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings)
                            : Loli2CSharpTranspiler.Transpile(Config.LoliCodeScript, Config.Settings);

                    Config.StartupCSharpScript = Loli2CSharpTranspiler.Transpile(
                        Config.StartupLoliCodeScript, Config.Settings);

                    editor.Text = Config.CSharpScript;
                    startupEditor.Text = Config.StartupCSharpScript;

                    if (configService.SelectedConfig.StartupCSharpScript is not null &&
                        configService.SelectedConfig.StartupCSharpScript.Length > 0)
                    {
                        startupEditorContainer.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                // On fail, prompt it to the user and go back to the configs page
                Alert.Exception(ex);
                SP.GetService<MainWindow>().NavigateTo(MainWindowPage.Configs);
            }
        }

        private void HighlightSyntax(TextEditor textEditor)
        {
            using var reader = XmlReader.Create("Highlighting/LoliCode.xshd");
            textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            textEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            textEditor.TextArea.TextView.LinkTextUnderline = false;
        }

        private void ToggleUsings(object sender, RoutedEventArgs e) => usingsContainer.Visibility =
            usingsContainer.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

        private void ToggleStartup(object sender, RoutedEventArgs e) => startupEditorContainer.Visibility =
            startupEditorContainer.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
    }

    public class ConfigCSharpCodeViewModel : ViewModelBase
    {
        private readonly ConfigService configService;
        private readonly OpenBulletSettingsService obSettingsService;
        private Config Config => configService.SelectedConfig;

        public ConfigCSharpCodeViewModel()
        {
            configService = SP.GetService<ConfigService>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
        }

        public bool WordWrap => obSettingsService.Settings.CustomizationSettings.WordWrap;

        public string UsingsString
        {
            get => string.Join(Environment.NewLine, Config.Settings.ScriptSettings.CustomUsings);
            set
            {
                Config.Settings.ScriptSettings.CustomUsings = value.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
                OnPropertyChanged();
            }
        }
    }
}
