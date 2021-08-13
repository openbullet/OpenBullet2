using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigSettings.xaml
    /// </summary>
    public partial class ConfigSettings : Page
    {
        private readonly ConfigSettingsViewModel vm;

        public ConfigSettings()
        {
            vm = SP.GetService<ViewModelsService>().ConfigSettings;
            DataContext = vm;

            InitializeComponent();
        }

        public void UpdateViewModel() => vm.UpdateViewModel();
    }
}
