using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Services;
using Microsoft.Extensions.Logging;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using MonitorAction = RuriLib.Models.Jobs.Monitor.Actions.Action;
using MonitorTrigger = RuriLib.Models.Jobs.Monitor.Triggers.Trigger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for Monitor.xaml
/// </summary>
public partial class Monitor : Page
{
    private readonly IUiFactory uiFactory;
    private readonly MainWindow mainWindow;
    private readonly MonitorViewModel vm;

    public Monitor(
        IUiFactory uiFactory,
        MainWindow mainWindow,
        ILogger<MonitorViewModel> logger,
        JobMonitorService jobMonitorService,
        JobManagerService jobManagerService)
    {
        this.uiFactory = uiFactory;
        this.mainWindow = mainWindow;
        vm = new MonitorViewModel(logger, jobMonitorService, jobManagerService);
        DataContext = vm;
        InitializeComponent();
    }

    public void UpdateViewModel() => vm.Reload();

    public Task CreateTriggeredActionAsync(TriggeredAction action)
    {
        vm.Create(action);
        return Task.CompletedTask;
    }

    public Task UpdateTriggeredActionAsync(string originalId, TriggeredAction action)
    {
        vm.Update(originalId, action);
        return Task.CompletedTask;
    }

    private void CreateTriggeredAction(object sender, RoutedEventArgs e)
        => OpenEditor(TriggeredActionEditMode.Create, null);

    private void ReloadTriggeredActions(object sender, RoutedEventArgs e)
    {
        try
        {
            vm.Reload();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void EditTriggeredAction(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TriggeredActionItemViewModel item })
        {
            OpenEditor(TriggeredActionEditMode.Edit, item.Action);
        }
    }

    private void CloneTriggeredAction(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: TriggeredActionItemViewModel item })
        {
            OpenEditor(TriggeredActionEditMode.Clone, item.Action);
        }
    }

    private void DeleteTriggeredAction(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { Tag: TriggeredActionItemViewModel item })
            {
                return;
            }

            if (!Alert.Choice("Delete triggered action",
                $"Are you sure you want to delete the triggered action {item.Action.Name}?"))
            {
                return;
            }

            vm.Delete(item.Action.Id);
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void ToggleTriggeredAction(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { Tag: TriggeredActionItemViewModel item })
            {
                vm.SetActive(item.Action.Id, !item.Action.IsActive);
            }
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void ResetTriggeredAction(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button { Tag: TriggeredActionItemViewModel item })
            {
                vm.Reset(item.Action.Id);
            }
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void ViewJob(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not Button { Tag: TriggeredActionItemViewModel item })
            {
                return;
            }

            var job = vm.GetJob(item.Action.JobId);

            if (job is null)
            {
                Alert.Error("Invalid job", $"The monitored job #{item.Action.JobId} was not found");
                return;
            }

            JobViewModel jobViewModel = job switch
            {
                MultiRunJob multiRunJob => new MultiRunJobViewModel(multiRunJob),
                ProxyCheckJob proxyCheckJob => new ProxyCheckJobViewModel(proxyCheckJob),
                _ => throw new NotImplementedException()
            };

            mainWindow.DisplayJob(jobViewModel);
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }

    private void OpenEditor(TriggeredActionEditMode mode, TriggeredAction? action)
    {
        var title = mode switch
        {
            TriggeredActionEditMode.Create => "Create Triggered Action",
            TriggeredActionEditMode.Edit => "Edit Triggered Action",
            TriggeredActionEditMode.Clone => "Clone Triggered Action",
            _ => throw new NotImplementedException()
        };

        Page dialog = action is null
            ? uiFactory.Create<EditTriggeredActionDialog>(this, vm.GetSupportedJobs(), mode)
            : uiFactory.Create<EditTriggeredActionDialog>(this, vm.GetSupportedJobs(), mode, action);

        new MainDialog(dialog, title, 1000, 850).ShowDialog();
    }
}

public class MonitorViewModel : ViewModelBase
{
    private readonly ILogger<MonitorViewModel> logger;
    private readonly JobMonitorService jobMonitorService;
    private readonly JobManagerService jobManagerService;
    private readonly Timer timer;

    private ObservableCollection<TriggeredActionItemViewModel> triggeredActionsCollection = [];
    public ObservableCollection<TriggeredActionItemViewModel> TriggeredActionsCollection
    {
        get => triggeredActionsCollection;
        private set
        {
            triggeredActionsCollection = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasTriggeredActions));
            OnPropertyChanged(nameof(HasNoTriggeredActions));
        }
    }

    public bool HasTriggeredActions => TriggeredActionsCollection.Count > 0;
    public bool HasNoTriggeredActions => !HasTriggeredActions;

    public MonitorViewModel(
        ILogger<MonitorViewModel> logger,
        JobMonitorService jobMonitorService,
        JobManagerService jobManagerService)
    {
        this.logger = logger;
        this.jobMonitorService = jobMonitorService;
        this.jobManagerService = jobManagerService;
        Reload();

        timer = new Timer(_ => Application.Current.Dispatcher.Invoke(RefreshItems), null, 1000, 1000);
    }

    public void Reload()
    {
        TriggeredActionsCollection = new ObservableCollection<TriggeredActionItemViewModel>(
            jobMonitorService.TriggeredActions.Select(a => new TriggeredActionItemViewModel(a, jobManagerService)));
        logger.LogDebug("Reloaded {TriggeredActionCount} triggered action(s)", TriggeredActionsCollection.Count);
    }

    public void Create(TriggeredAction action)
    {
        jobMonitorService.TriggeredActions.Add(action);
        jobMonitorService.SaveStateIfChanged();
        Reload();
        logger.LogInformation("Created triggered action {TriggeredActionId} for job {JobId}", action.Id, action.JobId);
    }

    public void Update(string originalId, TriggeredAction action)
    {
        var index = jobMonitorService.TriggeredActions.FindIndex(a => a.Id == originalId);

        if (index < 0)
        {
            throw new InvalidOperationException($"Could not find triggered action {originalId}");
        }

        jobMonitorService.TriggeredActions[index] = action;
        jobMonitorService.SaveStateIfChanged();
        Reload();
        logger.LogInformation("Updated triggered action {TriggeredActionId} for job {JobId}", action.Id, action.JobId);
    }

    public void Delete(string id)
    {
        var action = GetAction(id);
        jobMonitorService.TriggeredActions.Remove(action);
        jobMonitorService.SaveStateIfChanged();
        Reload();
        logger.LogInformation("Deleted triggered action {TriggeredActionId} for job {JobId}", action.Id, action.JobId);
    }

    public void Reset(string id)
    {
        var action = GetAction(id);
        action.Reset();
        jobMonitorService.SaveStateIfChanged();
        RefreshItems();
        logger.LogInformation("Reset triggered action {TriggeredActionId}", action.Id);
    }

    public void SetActive(string id, bool active)
    {
        var action = GetAction(id);
        action.IsActive = active;
        jobMonitorService.SaveStateIfChanged();
        RefreshItems();
        logger.LogInformation("Set triggered action {TriggeredActionId} active state to {IsActive}", action.Id, active);
    }

    public Job? GetJob(int id) => jobManagerService.Jobs.FirstOrDefault(j => j.Id == id);

    public IReadOnlyList<Job> GetSupportedJobs()
        => jobManagerService.Jobs
            .Where(j => j is MultiRunJob or ProxyCheckJob)
            .OrderBy(j => j.Id)
            .ToList();

    private TriggeredAction GetAction(string id)
        => jobMonitorService.TriggeredActions.FirstOrDefault(a => a.Id == id)
            ?? throw new InvalidOperationException($"Could not find triggered action {id}");

    private void RefreshItems()
    {
        foreach (var action in TriggeredActionsCollection)
        {
            action.UpdateViewModel();
        }
    }
}

public class TriggeredActionItemViewModel(TriggeredAction action, JobManagerService jobManagerService) : ViewModelBase
{
    private readonly JobManagerService jobManagerService = jobManagerService;

    public TriggeredAction Action { get; } = action;

    public string DisplayName
    {
        get
        {
            var suffix = Action.IsExecuting
                ? " (executing)"
                : Action.IsActive ? string.Empty : " (disabled)";

            return $"{Action.Name}{suffix}";
        }
    }

    public string JobDisplayName
    {
        get
        {
            var job = jobManagerService.Jobs.FirstOrDefault(j => j.Id == Action.JobId);

            if (job is null)
            {
                return $"Job #{Action.JobId} - Invalid job";
            }

            return $"Job #{job.Id} - {GetJobName(job)} ({GetJobTypeText(job)})";
        }
    }

    public string ExecutionText => Action.IsRepeatable
        ? $"Executions: {Action.Executions}"
        : $"Executed: {(Action.Executions > 0 ? "Yes" : "No")}";

    public bool IsEnabled => Action.IsActive;
    public bool IsRepeatable => Action.IsRepeatable;
    public bool WasExecuted => Action.Executions > 0;
    public bool HasExecutedBoolean => !Action.IsRepeatable;
    public bool HasExecutionCount => Action.IsRepeatable;
    public bool CanReset => !Action.IsRepeatable && Action.Executions > 0;
    public string ToggleButtonToolTip => Action.IsActive ? "Disable this triggered action" : "Enable this triggered action";
    public double ItemOpacity => Action.IsActive ? 1d : 0.6d;

    public IReadOnlyList<string> TriggerDescriptions => Action.Triggers.Select(MonitorTextFormatter.GetTriggerText).ToList();
    public IReadOnlyList<string> ActionDescriptions => Action.Actions.Select(MonitorTextFormatter.GetActionText).ToList();

    public override void UpdateViewModel()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(JobDisplayName));
        OnPropertyChanged(nameof(ExecutionText));
        OnPropertyChanged(nameof(IsEnabled));
        OnPropertyChanged(nameof(IsRepeatable));
        OnPropertyChanged(nameof(WasExecuted));
        OnPropertyChanged(nameof(HasExecutedBoolean));
        OnPropertyChanged(nameof(HasExecutionCount));
        OnPropertyChanged(nameof(CanReset));
        OnPropertyChanged(nameof(ToggleButtonToolTip));
        OnPropertyChanged(nameof(ItemOpacity));
        OnPropertyChanged(nameof(TriggerDescriptions));
        OnPropertyChanged(nameof(ActionDescriptions));
    }

    private static string GetJobName(Job job) => string.IsNullOrWhiteSpace(job.Name) ? "Unnamed" : job.Name;

    private static string GetJobTypeText(Job job) => job switch
    {
        MultiRunJob => "Multi Run",
        ProxyCheckJob => "Proxy Check",
        _ => "Unknown"
    };
}

internal static class MonitorTextFormatter
{
    public static string GetTriggerText(MonitorTrigger trigger) => trigger switch
    {
        JobStatusTrigger t => $"Job status is {t.Status}",
        JobFinishedTrigger => "Job finished",
        TestedCountTrigger t => $"Tested count {GetComparisonText(t.Comparison)} {t.Amount}",
        HitCountTrigger t => $"Hit count {GetComparisonText(t.Comparison)} {t.Amount}",
        CustomCountTrigger t => $"Custom count {GetComparisonText(t.Comparison)} {t.Amount}",
        ToCheckCountTrigger t => $"To check count {GetComparisonText(t.Comparison)} {t.Amount}",
        FailCountTrigger t => $"Fail count {GetComparisonText(t.Comparison)} {t.Amount}",
        RetryCountTrigger t => $"Retry count {GetComparisonText(t.Comparison)} {t.Amount}",
        BanCountTrigger t => $"Ban count {GetComparisonText(t.Comparison)} {t.Amount}",
        ErrorCountTrigger t => $"Error count {GetComparisonText(t.Comparison)} {t.Amount}",
        AliveProxiesCountTrigger t => $"Alive proxies count {GetComparisonText(t.Comparison)} {t.Amount}",
        BannedProxiesCountTrigger t => $"Banned proxies count {GetComparisonText(t.Comparison)} {t.Amount}",
        CPMTrigger t => $"CPM {GetComparisonText(t.Comparison)} {t.Amount}",
        CaptchaCreditTrigger t => $"Captcha credit {GetComparisonText(t.Comparison)} {t.Amount}",
        ProgressTrigger t => $"Progress {GetComparisonText(t.Comparison)} {t.Amount}%",
        TimeElapsedTrigger t => $"Time elapsed {GetComparisonText(t.Comparison)} {GetTimeSpanText(t.Days, t.Hours, t.Minutes, t.Seconds)}",
        TimeRemainingTrigger t => $"Time remaining {GetComparisonText(t.Comparison)} {GetTimeSpanText(t.Days, t.Hours, t.Minutes, t.Seconds)}",
        _ => $"Unknown trigger ({trigger.GetType().Name})"
    };

    public static string GetActionText(MonitorAction action) => action switch
    {
        WaitAction a => $"Wait {GetTimeSpanText(a.Days, a.Hours, a.Minutes, a.Seconds)}",
        SetRelativeStartConditionAction a => $"Set relative start condition of job {a.JobId} to {GetTimeSpanText(a.Days, a.Hours, a.Minutes, a.Seconds)}",
        StopJobAction a => $"Stop job {a.JobId}",
        AbortJobAction a => $"Abort job {a.JobId}",
        StartJobAction a => $"Start job {a.JobId}",
        DiscordWebhookAction => "Send message via Discord webhook",
        TelegramBotAction => "Send message via Telegram bot",
        SetBotsAction a => $"Set bots to {a.Amount}",
        SetSkipAction a => $"Set skip to {a.Skip}",
        ReloadProxiesAction => "Reload proxies",
        _ => $"Unknown action ({action.GetType().Name})"
    };

    private static string GetComparisonText(NumComparison comparison) => comparison switch
    {
        NumComparison.EqualTo => "=",
        NumComparison.NotEqualTo => "!=",
        NumComparison.LessThan => "<",
        NumComparison.LessThanOrEqualTo => "<=",
        NumComparison.GreaterThan => ">",
        NumComparison.GreaterThanOrEqualTo => ">=",
        _ => comparison.ToString()
    };

    private static string GetTimeSpanText(int days, int hours, int minutes, int seconds)
    {
        var timeSpan = new TimeSpan(days, hours, minutes, seconds);

        return days > 0
            ? $"{days} day(s) {timeSpan:hh\\:mm\\:ss}"
            : timeSpan.ToString();
    }
}
