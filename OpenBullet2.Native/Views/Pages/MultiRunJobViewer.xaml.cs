using OpenBullet2.Native.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for MultiRunJobViewer.xaml
    /// </summary>
    public partial class MultiRunJobViewer : Page
    {
        private MultiRunJobViewerViewModel vm;

        public MultiRunJobViewer()
        {
            InitializeComponent();
        }

        public void UpdateViewModel(MultiRunJobViewModel jobVM)
        {
            vm?.Dispose();

            vm = new MultiRunJobViewerViewModel(jobVM);
            DataContext = vm;
        }

        private void Start(object sender, RoutedEventArgs e) { }
        private void Stop(object sender, RoutedEventArgs e) { }
        private void Pause(object sender, RoutedEventArgs e) { }
        private void Resume(object sender, RoutedEventArgs e) { }
        private void Abort(object sender, RoutedEventArgs e) { }
        private void SkipWait(object sender, RoutedEventArgs e) { }
    }
}
