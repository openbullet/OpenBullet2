using Newtonsoft.Json;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Jobs.xaml
    /// </summary>
    public partial class Jobs : Page
    {
        private readonly MainWindow mainWindow;
        private readonly IJobRepository jobRepo;
        private readonly JobsViewModel vm;

        public Jobs()
        {
            mainWindow = SP.GetService<MainWindow>();
            jobRepo = SP.GetService<IJobRepository>();
            vm = SP.GetService<ViewModelsService>().Jobs;
            DataContext = vm;

            InitializeComponent();
        }

        private void NewJob(object sender, RoutedEventArgs e)
            => new MainDialog(new CreateJobDialog(this), "Select job type").ShowDialog();

        private void RemoveAll(object sender, RoutedEventArgs e)
        {
            try
            {
                vm.RemoveAll();
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private void EditJob(object sender, RoutedEventArgs e) => EditJob((JobViewModel)(sender as Button).Tag);

        public async void EditJob(JobViewModel jobVM)
        {
            var entity = await jobRepo.Get(jobVM.Id);
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, jsonSettings).Options;
            Action<JobOptions> onAccept = async options =>
            {
                jobVM = await vm.EditJob(entity, options);
                mainWindow.DisplayJob(jobVM);
            };

            Page page = jobVM switch
            {
                MultiRunJobViewModel => new MultiRunJobOptionsDialog(jobOptions as MultiRunJobOptions, onAccept),
                ProxyCheckJobViewModel => new ProxyCheckJobOptionsDialog(jobOptions as ProxyCheckJobOptions, onAccept),
                _ => throw new NotImplementedException()
            };

            new MainDialog(page, $"Edit job #{entity.Id}").ShowDialog();
        }

        private async void CloneJob(object sender, RoutedEventArgs e)
        {
            var jobVM = (JobViewModel)(sender as Button).Tag;
            var entity = await jobRepo.Get(jobVM.Id);
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var oldOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, jsonSettings).Options;
            var newOptions = JobOptionsFactory.CloneExistant(oldOptions);

            Action<JobOptions> onAccept = async options =>
            {
                var cloned = await vm.CloneJob(entity.JobType, options);
                mainWindow.DisplayJob(cloned);
            };

            Page page = jobVM switch
            {
                MultiRunJobViewModel => new MultiRunJobOptionsDialog(newOptions as MultiRunJobOptions, onAccept),
                ProxyCheckJobViewModel => new ProxyCheckJobOptionsDialog(newOptions as ProxyCheckJobOptions, onAccept),
                _ => throw new NotImplementedException()
            };

            new MainDialog(page, $"Clone job #{entity.Id}").ShowDialog();
        }

        private async void RemoveJob(object sender, RoutedEventArgs e)
        {
            try
            {
                await vm.RemoveJob((JobViewModel)(sender as Button).Tag);
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        public async void CreateJob(JobOptions options) => await vm.CreateJob(options);

        private void ViewJob(object sender, MouseButtonEventArgs e)
            => SP.GetService<MainWindow>().DisplayJob((JobViewModel)(sender as WrapPanel).Tag);
    }
}
