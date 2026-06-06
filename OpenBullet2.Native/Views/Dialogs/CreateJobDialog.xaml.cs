using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Views.Pages;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for CreateJobDialog.xaml
/// </summary>
public partial class CreateJobDialog : Page
{
    private readonly object caller;
    private readonly IUiFactory uiFactory;

    public CreateJobDialog(object caller, IUiFactory uiFactory)
    {
        this.caller = caller;
        this.uiFactory = uiFactory;

        InitializeComponent();
    }

    private void CreateMultiRunJob(object sender, RoutedEventArgs e) => CreateJob(JobType.MultiRun);
    private void CreateProxyCheckJob(object sender, RoutedEventArgs e) => CreateJob(JobType.ProxyCheck);

    private void CreateJob(JobType type)
    {
        Func<JobOptions, Task>? onAccept = caller is Jobs page
            ? page.CreateJobAsync
            : null;

        switch (type)
        {
            case JobType.MultiRun:
                new MainDialog(uiFactory.Create<MultiRunJobOptionsDialog>(onAccept!), "Create Multi Run Job", 800, 600).ShowDialog();
                break;

            case JobType.ProxyCheck:
                new MainDialog(uiFactory.Create<ProxyCheckJobOptionsDialog>(onAccept!), "Create Proxy Check Job").ShowDialog();
                break;
        }

        ((MainDialog)Parent).Close();
    }
}
