using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Views.Pages.Shared;
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

        public ConfigLoliCode()
        {
            InitializeComponent();
            configService = SP.GetService<ConfigService>();
            configRepo = SP.GetService<IConfigRepository>();
            debuggerFrame.Content = SP.GetService<Debugger>();

            HighlightSyntax();
            UpdateViewModel();
        }

        public void UpdateViewModel() => editor.Text = configService.SelectedConfig.LoliCodeScript;

        private void EditorLostFocus(object sender, RoutedEventArgs e) 
            => configService.SelectedConfig.LoliCodeScript = editor.Text;

        private void HighlightSyntax()
        {
            using var reader = XmlReader.Create("LoliCode.xshd");
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
    }
}
