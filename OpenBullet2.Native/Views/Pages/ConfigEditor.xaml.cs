using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages.Shared;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigEditor.xaml
    /// </summary>
    public partial class ConfigEditor : Page
    {
        private readonly MainWindow mainWindow;
        private readonly ConfigEditorViewModel vm;
        private readonly Debugger debugger;
        private readonly ConfigStacker stackerPage;
        private readonly ConfigLoliCode loliCodePage;
        private readonly ConfigCSharpCode cSharpPage;

        public ConfigEditor()
        {
            mainWindow = SP.GetService<MainWindow>();
            vm = new ConfigEditorViewModel();
            DataContext = vm;

            InitializeComponent();

            editorFrame.Navigated += (_, _) => UpdateButtonsVisibility();

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

        private void UpdateButtonsVisibility()
        {
            if (vm.Config.Mode == ConfigMode.Stack || vm.Config.Mode == ConfigMode.LoliCode)
            {
                stackerButton.Visibility = editorFrame.Content != stackerPage
                    ? Visibility.Visible : Visibility.Collapsed;

                loliCodeButton.Visibility = editorFrame.Content != loliCodePage
                    ? Visibility.Visible : Visibility.Collapsed;

                cSharpButton.Visibility = editorFrame.Content != cSharpPage ? Visibility.Visible : Visibility.Collapsed;
            }
            else // C# only mode
            {
                stackerButton.Visibility = Visibility.Collapsed;
                loliCodeButton.Visibility = Visibility.Collapsed;
                cSharpButton.Visibility = Visibility.Collapsed;
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

        private void OpenStacker(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigStacker);
        private void OpenLoliCode(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigLoliCode);
        private void OpenCSharpCode(object sender, RoutedEventArgs e) => mainWindow.NavigateTo(MainWindowPage.ConfigCSharpCode);
        
        private async void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                await vm.Save();
                Alert.Success("Success", $"{vm.Config.Metadata.Name} was saved successfully!");
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }

    public enum ConfigEditorSection
    {
        Stacker,
        LoliCode,
        CSharp
    }

    public class ConfigEditorViewModel : ViewModelBase
    {
        private readonly IConfigRepository configRepo;
        private readonly ConfigService configService;
        public Config Config => configService.SelectedConfig;

        public ConfigEditorViewModel()
        {
            configRepo = SP.GetService<IConfigRepository>();
            configService = SP.GetService<ConfigService>();
        }

        public Task Save() => configRepo.Save(Config);
    }
}
