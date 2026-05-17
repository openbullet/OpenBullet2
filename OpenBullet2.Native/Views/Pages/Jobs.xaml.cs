using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Jobs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for Jobs.xaml
/// </summary>
public partial class Jobs : Page
{
    private readonly ILogger<Jobs> logger;
    private readonly IUiFactory uiFactory;
    private readonly MainWindow mainWindow;
    private readonly JobsViewModel vm;
    private readonly IServiceScopeFactory scopeFactory;
    private static readonly JsonSerializerSettings JsonSettings = new() { TypeNameHandling = TypeNameHandling.Auto };

    public Jobs(
        ILogger<Jobs> logger,
        IUiFactory uiFactory,
        MainWindow mainWindow,
        JobsViewModel vm,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.uiFactory = uiFactory;
        this.mainWindow = mainWindow;
        this.vm = vm;
        this.scopeFactory = scopeFactory;
        DataContext = vm;

        InitializeComponent();
    }

    private void NewJob(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<CreateJobDialog>(this), "Select job type").ShowDialog();

    private void RemoveAll(object sender, RoutedEventArgs e)
    {
        try
        {
            vm.RemoveAll();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove all jobs from Native jobs page");
            Alert.Exception(ex);
        }
    }

    private async void EditJob(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { Tag: JobViewModel job })
            {
                await EditJobAsync(job);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to edit job from Native jobs page");
            Alert.Exception(ex);
        }
    }

    public async Task EditJobAsync(JobViewModel jobVM)
    {
        if (jobVM.Status != JobStatus.Idle)
        {
            Alert.Warning("Cannot edit job", "Stop or abort the job before editing it.");
            return;
        }

        var entity = await GetJobEntityAsync(jobVM.Id);
        var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions ?? string.Empty, JsonSettings)?.Options
            ?? throw new InvalidOperationException("Could not deserialize job options");
        async Task OnAcceptAsync(JobOptions options)
        {
            jobVM = await vm.EditJobAsync(entity, options);
            mainWindow.DisplayJob(jobVM);
        }

        Page page = jobVM switch
        {
            MultiRunJobViewModel => uiFactory.Create<MultiRunJobOptionsDialog>((MultiRunJobOptions)jobOptions, (Func<JobOptions, Task>)OnAcceptAsync),
            ProxyCheckJobViewModel => uiFactory.Create<ProxyCheckJobOptionsDialog>((ProxyCheckJobOptions)jobOptions, (Func<JobOptions, Task>)OnAcceptAsync),
            _ => throw new NotImplementedException()
        };

        new MainDialog(page, $"Edit job #{entity.Id}", 800, 600).ShowDialog();
    }

    private async void CloneJob(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: JobViewModel jobVM })
        {
            return;
        }

        var entity = await GetJobEntityAsync(jobVM.Id);
        var oldOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions ?? string.Empty, JsonSettings)?.Options
            ?? throw new InvalidOperationException("Could not deserialize job options");
        var newOptions = JobOptionsFactory.CloneExistant(oldOptions);

        async Task OnAcceptAsync(JobOptions options)
        {
            var cloned = await vm.CloneJobAsync(entity.JobType, options);
            mainWindow.DisplayJob(cloned);
        }

        Page page = jobVM switch
        {
            MultiRunJobViewModel => uiFactory.Create<MultiRunJobOptionsDialog>((MultiRunJobOptions)newOptions, (Func<JobOptions, Task>)OnAcceptAsync),
            ProxyCheckJobViewModel => uiFactory.Create<ProxyCheckJobOptionsDialog>((ProxyCheckJobOptions)newOptions, (Func<JobOptions, Task>)OnAcceptAsync),
            _ => throw new NotImplementedException()
        };

        new MainDialog(page, $"Clone job #{entity.Id}", 800, 600).ShowDialog();
    }

    private async void RemoveJob(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { Tag: JobViewModel job })
            {
                await vm.RemoveJobAsync(job);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove job from Native jobs page");
            Alert.Exception(ex);
        }
    }

    public Task CreateJobAsync(JobOptions options) => vm.CreateJobAsync(options);

    private void ViewJob(object sender, MouseButtonEventArgs e)
    {
        if (sender is WrapPanel { Tag: JobViewModel job })
        {
            mainWindow.DisplayJob(job);
        }
    }

    private async Task<OpenBullet2.Core.Entities.JobEntity> GetJobEntityAsync(int id)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        return await repo.GetAsync(id);
    }
}
