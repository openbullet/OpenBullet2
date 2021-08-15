using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Views.Pages.Shared;
using System.Windows;
using System.Windows.Controls;
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

        public ConfigLoliCode()
        {
            InitializeComponent();
            configService = SP.GetService<ConfigService>();
            debuggerFrame.Content = SP.GetService<Debugger>();

            HighlightSyntax();
            UpdateViewModel();
        }

        public void UpdateViewModel()
        {
            editor.Text = configService.SelectedConfig.LoliCodeScript;
        }

        private void EditorLostFocus(object sender, RoutedEventArgs e) 
            => configService.SelectedConfig.LoliCodeScript = editor.Text;

        private void HighlightSyntax()
        {
            using var reader = XmlReader.Create("LoliCode.xshd");
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            editor.TextArea.TextView.LinkTextUnderline = false;
        }
    }
}
