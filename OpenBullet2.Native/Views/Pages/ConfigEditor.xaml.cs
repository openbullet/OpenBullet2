using OpenBullet2.Native.Views.Pages.Shared;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigEditor.xaml
    /// </summary>
    public partial class ConfigEditor : Page
    {
        private readonly Debugger debugger;
        private readonly ConfigStacker stackerPage;
        private readonly ConfigLoliCode loliCodePage;
        private readonly ConfigCSharpCode cSharpPage;

        public ConfigEditor()
        {
            InitializeComponent();

            // Create the pages
            debugger = new();
            stackerPage = new();
            loliCodePage = new();
            cSharpPage = new();

            debuggerFrame.Content = debugger;
        }

        public void NavigateTo(ConfigEditorSection section)
        {
            switch (section)
            {
                case ConfigEditorSection.Stacker:
                    stackerPage.UpdateViewModel();
                    editorFrame.Content = stackerPage;
                    break;

                case ConfigEditorSection.LoliCode:
                    loliCodePage.UpdateViewModel();
                    editorFrame.Content = loliCodePage;
                    break;

                case ConfigEditorSection.CSharp:
                    cSharpPage.UpdateViewModel();
                    editorFrame.Content = cSharpPage;
                    break;

                default:
                    break;
            }
        }
    }

    public enum ConfigEditorSection
    {
        Stacker,
        LoliCode,
        CSharp
    }
}
