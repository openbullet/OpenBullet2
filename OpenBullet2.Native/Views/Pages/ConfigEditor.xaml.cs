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

        /// <summary>
        /// Call this when changing page via the dropdown menu otherwise it
        /// will not save the content of the LoliCode editor.
        /// </summary>
        public void OnPageChanged()
        {
            if (editorFrame.Content == loliCodePage)
            {
                loliCodePage.OnPageChanged();
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
