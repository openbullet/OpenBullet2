using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
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
        private readonly ConfigService configService;
        private readonly OpenBulletSettingsService obSettingsService;
        private Config Config => configService.SelectedConfig;

        public ConfigCSharpCode()
        {
            InitializeComponent();
            configService = SP.GetService<ConfigService>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();

            HighlightSyntax();
            editor.WordWrap = obSettingsService.Settings.CustomizationSettings.WordWrap;
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

                    editor.Text = Config.CSharpScript;
                }
            }
            catch (Exception ex)
            {
                // On fail, prompt it to the user and go back to the configs page
                Alert.Exception(ex);
                SP.GetService<MainWindow>().NavigateTo(MainWindowPage.Configs);
            }
        }

        private void HighlightSyntax()
        {
            using var reader = XmlReader.Create("Highlighting/CSharp.xshd");
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            editor.TextArea.TextView.LinkTextUnderline = false;
        }
    }
}
