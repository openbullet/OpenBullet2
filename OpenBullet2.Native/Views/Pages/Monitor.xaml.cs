using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for Monitor.xaml
    /// </summary>
    public partial class Monitor : Page
    {
        private readonly MonitorViewModel vm;
        public Monitor()
        {
            vm = SP.GetService<ViewModelsService>().Monitor;
            DataContext = vm;
            InitializeComponent();
        }

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
        private void CreateTriggeredAction(object sender, RoutedEventArgs e) => vm.CreateTriggeredAction();
        private void ResetTriggeredAction(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).Reset();

        public void AddJobStatusTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new JobStatusTrigger());
        public void AddTimeElapsedTrigger(object sender, RoutedEventArgs e) => new MainDialog(new AddTimeElapsedTriggerDialog((TriggeredActionViewModel)(sender as Button).Tag), "Time Elapsed Trigger").ShowDialog();
        public void AddJobFinishedTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new JobFinishedTrigger());
        public void AddHitCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new HitCountTrigger());
        public void AddCPMCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new CPMTrigger());
        public void AddToCheckCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new ToCheckCountTrigger());
        public void AddRetryCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new RetryCountTrigger());
        public void AddBanCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new BanCountTrigger());
        public void AddTestedCountTrigger(object sender, RoutedEventArgs e) => ((TriggeredActionViewModel)(sender as Button).Tag).AddTrigger(new TestedCountTrigger());

        private void AddAbortJobAction(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Match job id by default for convinience
            var action = new AbortJobAction() { 
                JobId = tr.JobId,
            };

            tr.AddAction(action);
        }

        private void AddStopJobAction(object sender, RoutedEventArgs e) {

            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Match job id by default for convinience
            var action = new StopJobAction()
            {
                JobId = tr.JobId,
            };

            tr.AddAction(action);
        }

        private void AddStartJobAction(object sender, RoutedEventArgs e) {

            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Match job id by default for convinience
            var action = new StartJobAction()
            {
                JobId = tr.JobId,
            };

            tr.AddAction(action);
        }

        private void AddWaitAction(object sender, RoutedEventArgs e) => new MainDialog(new AddWaitTimeActionDialog((TriggeredActionViewModel)(sender as Button).Tag), "Wait Action").ShowDialog();

        private void AddSetBotsAction(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Match job id by default for convinience
            var action = new SetBotsAction()
            {
                TargetJobId = tr.JobId,
            };

            tr.AddAction(action);
        }

        private void AddTelegramWebhookAction(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Set default api server for conviniecne
            var action = new TelegramBotAction()
            {
                ApiServer = "https://api.telegram.org/"
            };

            tr.AddAction(action);
        }

        private void AddDiscordWebhookAction(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel tr)
                return;

            // Set default api server for conviniecne
            var action = new DiscordWebhookAction();

            tr.AddAction(action);
        }

        private void DuplicateTriggeredAction(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).Tag;

            if (tag is not TriggeredActionViewModel oldTr)
                return;

            if (oldTr != null)
            {
                TriggeredAction newTr = vm.CloneTriggeredAction(oldTr);
                vm.AddTriggeredAction(newTr);
            }
        }

        private void RemoveTriggeredAction(object sender, RoutedEventArgs e) => vm.RemoveTriggeredAction((TriggeredActionViewModel)(sender as Button).Tag);
    }
}
