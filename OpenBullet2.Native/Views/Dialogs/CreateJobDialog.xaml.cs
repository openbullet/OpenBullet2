using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateJobDialog.xaml
    /// </summary>
    public partial class CreateJobDialog : Page
    {
        private readonly object caller;

        public CreateJobDialog(object caller)
        {
            this.caller = caller;

            InitializeComponent();
        }

        private void CreateMultiRunJob(object sender, RoutedEventArgs e) => CreateJob(JobType.MultiRun);
        private void CreateProxyCheckJob(object sender, RoutedEventArgs e) => CreateJob(JobType.ProxyCheck);

        private void CreateJob(JobType type)
        {
            Action<JobOptions> onAccept = options =>
            {
                if (caller is Jobs page)
                {
                    page.CreateJob(options);
                }
            };

            switch (type)
            {
                case JobType.MultiRun:
                    new MainDialog(new MultiRunJobOptionsDialog(null, onAccept), "Create Multi Run Job").ShowDialog();
                    break;

                case JobType.ProxyCheck:
                    new MainDialog(new ProxyCheckJobOptionsDialog(null, onAccept), "Create Proxy Check Job").ShowDialog();
                    break;
            }

            ((MainDialog)Parent).Close();
        }
    }
}
