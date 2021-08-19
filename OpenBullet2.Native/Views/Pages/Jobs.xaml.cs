using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Jobs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Jobs.xaml
    /// </summary>
    public partial class Jobs : Page
    {
        private readonly JobsViewModel vm;

        public Jobs()
        {
            vm = SP.GetService<ViewModelsService>().Jobs;
            DataContext = vm;

            InitializeComponent();
        }

        private void NewJob(object sender, RoutedEventArgs e)
            => new MainDialog(new CreateJobDialog(this), "Select job type").ShowDialog();

        private void RemoveAll(object sender, RoutedEventArgs e) { }
        private void EditJob(object sender, RoutedEventArgs e) { }
        private void CloneJob(object sender, RoutedEventArgs e) { }

        private async void RemoveJob(object sender, RoutedEventArgs e)
        {
            try
            {
                await vm.RemoveJob((Job)(sender as Button).Tag);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        public async void CreateJob(JobOptions options) => await vm.CreateJob(options);
    }
}
