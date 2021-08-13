using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigReadme.xaml
    /// </summary>
    public partial class ConfigReadme : Page
    {
        private readonly ConfigReadmeViewModel vm;

        public ConfigReadme()
        {
            vm = SP.GetService<ViewModelsService>().ConfigReadme;
            DataContext = vm;

            InitializeComponent();
        }

        public void UpdateViewModel()
        {
            vm.UpdateViewModel();
            readmeRTB.Document.Blocks.Clear();
            readmeRTB.AppendText(vm.Readme);
        }

        private void ReadmeChanged(object sender, TextChangedEventArgs e)
        {
            var newText = readmeRTB.GetText();

            if (!string.IsNullOrWhiteSpace(newText))
            {
                vm.Readme = newText;
            }
        }
    }
}
