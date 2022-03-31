using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for LoliCodeBlockSettingsViewer.xaml
    /// </summary>
    public partial class LoliCodeBlockSettingsViewer : UserControl
    {
        private readonly LoliCodeBlockSettingsViewerViewModel vm;
        private readonly OpenBulletSettingsService obSettingsService;

        public LoliCodeBlockSettingsViewer(BlockViewModel blockVM)
        {
            if (blockVM.Block is not LoliCodeBlockInstance)
            {
                throw new Exception("Wrong block type for this UC");
            }

            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            vm = new LoliCodeBlockSettingsViewerViewModel(blockVM);
            DataContext = vm;

            InitializeComponent();

            editor.WordWrap = obSettingsService.Settings.CustomizationSettings.WordWrap;
            editor.Text = vm.Script;
            HighlightSyntax();
        }

        private void EditorLostFocus(object sender, RoutedEventArgs e) => vm.Script = editor.Text;
        private void EditorTextChanged(object sender, EventArgs e) => vm.Script = editor.Text;

        private void HighlightSyntax()
        {
            using var reader = XmlReader.Create("Highlighting/LoliCode.xshd");
            editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Colors.DodgerBlue);
            editor.TextArea.TextView.LinkTextUnderline = false;
        }
    }

    public class LoliCodeBlockSettingsViewerViewModel : BlockSettingsViewerViewModel
    {
        public LoliCodeBlockInstance LoliCodeBlock => Block as LoliCodeBlockInstance;

        public string Script
        {
            get => LoliCodeBlock.Script;
            set
            {
                LoliCodeBlock.Script = value;
                OnPropertyChanged();
            }
        }

        public LoliCodeBlockSettingsViewerViewModel(BlockViewModel block) : base(block)
        {
            
        }
    }
}
