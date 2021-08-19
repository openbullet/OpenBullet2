using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiRunJobOptionsDialog.xaml
    /// </summary>
    public partial class MultiRunJobOptionsDialog : Page
    {
        private readonly Action<JobOptions> onAccept;
        private readonly MultiRunJobOptionsViewModel vm;

        public MultiRunJobOptionsDialog(MultiRunJobOptions options = null, Action<JobOptions> onAccept = null)
        {
            this.onAccept = onAccept;
            vm = new MultiRunJobOptionsViewModel(options);
            DataContext = vm;

            InitializeComponent();
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            onAccept?.Invoke(vm.Options);
            ((MainDialog)Parent).Close();
        }
    }

    public class MultiRunJobOptionsViewModel : ViewModelBase
    {
        public MultiRunJobOptions Options { get; init; }

        public MultiRunJobOptionsViewModel(MultiRunJobOptions options)
        {
            Options = options ?? new MultiRunJobOptions();
        }
    }
}
