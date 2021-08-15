using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages.Shared
{
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : Page
    {
        private readonly DebuggerViewModel vm;

        public Debugger()
        {
            vm = SP.GetService<ViewModelsService>().Debugger;
            DataContext = vm;

            InitializeComponent();
            tabControl.SelectedIndex = 0;
        }

        private void ShowLog(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 0;
        private void ShowVariables(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 1;
        private void ShowHTML(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 2;

        private void Start(object sender, RoutedEventArgs e) { }
        private void Stop(object sender, RoutedEventArgs e) { }
    }
}
