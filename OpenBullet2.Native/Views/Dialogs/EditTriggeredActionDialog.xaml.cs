using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages;
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
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for EditTriggeredActionDialog.xaml
/// </summary>
public partial class EditTriggeredActionDialog : Page
{
    private readonly object caller;
    private readonly EditTriggeredActionViewModel vm;

    public EditTriggeredActionDialog(object caller, IReadOnlyList<Job> jobs, TriggeredActionEditMode mode)
        : this(caller, jobs, mode, null)
    {
    }

    public EditTriggeredActionDialog(
        object caller,
        IReadOnlyList<Job> jobs,
        TriggeredActionEditMode mode,
        TriggeredAction? action)
    {
        this.caller = caller;
        vm = new EditTriggeredActionViewModel(jobs, mode, action);
        DataContext = vm;
        InitializeComponent();
    }

    private void AddTrigger(object sender, RoutedEventArgs e) => vm.AddSelectedTrigger();

    private void RemoveTrigger(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: MonitorTriggerEditorViewModel trigger })
        {
            vm.RemoveTrigger(trigger);
        }
    }

    private void AddAction(object sender, RoutedEventArgs e) => vm.AddSelectedAction();

    private void RemoveAction(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: MonitorActionEditorViewModel action })
        {
            vm.RemoveAction(action);
        }
    }

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!vm.TryValidate(out var message))
            {
                Alert.Error("Invalid triggered action", message);
                return;
            }

            var action = vm.BuildTriggeredAction();

            if (caller is Monitor page)
            {
                switch (vm.Mode)
                {
                    case TriggeredActionEditMode.Create:
                    case TriggeredActionEditMode.Clone:
                        await page.CreateTriggeredActionAsync(action);
                        break;

                    case TriggeredActionEditMode.Edit:
                        await page.UpdateTriggeredActionAsync(vm.OriginalId, action);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            ((MainDialog)Parent).Close();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
}

public enum TriggeredActionEditMode
{
    Create,
    Edit,
    Clone
}

public enum MonitorTriggerKind
{
    JobStatus,
    JobFinished,
    TestedCount,
    HitCount,
    CustomCount,
    ToCheckCount,
    FailCount,
    RetryCount,
    BanCount,
    ErrorCount,
    AliveProxiesCount,
    BannedProxiesCount,
    CpmCount,
    CaptchaCredit,
    Progress,
    TimeElapsed,
    TimeRemaining
}

public enum MonitorActionKind
{
    Wait,
    SetRelativeStartCondition,
    StopJob,
    AbortJob,
    StartJob,
    DiscordWebhook,
    TelegramBot,
    SetBots,
    SetSkip,
    ReloadProxies
}

public class EditTriggeredActionViewModel : ViewModelBase
{
    private readonly IReadOnlyList<MonitorJobOption> availableJobs;
    private readonly bool preserveRuntimeState;
    private readonly int originalExecutions;
    private readonly bool originalIsExecuting;

    public TriggeredActionEditMode Mode { get; }
    public string OriginalId { get; } = string.Empty;

    public ObservableCollection<MonitorJobOption> AvailableJobs { get; }
    public ObservableCollection<MonitorTriggerEditorViewModel> Triggers { get; } = [];
    public ObservableCollection<MonitorActionEditorViewModel> Actions { get; } = [];

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged();
        }
    }

    private bool isActive = true;
    public bool IsActive
    {
        get => isActive;
        set
        {
            isActive = value;
            OnPropertyChanged();
        }
    }

    private bool isRepeatable;
    public bool IsRepeatable
    {
        get => isRepeatable;
        set
        {
            isRepeatable = value;
            OnPropertyChanged();
        }
    }

    private int selectedJobId = -1;
    public int SelectedJobId
    {
        get => selectedJobId;
        set
        {
            selectedJobId = value;
            EnsureSelectedTypesAreValid();
            OnPropertyChanged();
            OnPropertyChanged(nameof(AvailableTriggerTypes));
            OnPropertyChanged(nameof(AvailableActionTypes));
        }
    }

    private MonitorTriggerKind selectedTriggerType;
    public MonitorTriggerKind SelectedTriggerType
    {
        get => selectedTriggerType;
        set
        {
            selectedTriggerType = value;
            OnPropertyChanged();
        }
    }

    private MonitorActionKind selectedActionType;
    public MonitorActionKind SelectedActionType
    {
        get => selectedActionType;
        set
        {
            selectedActionType = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<MonitorTriggerKind> AvailableTriggerTypes
    {
        get
        {
            var types = new List<MonitorTriggerKind>
            {
                MonitorTriggerKind.JobStatus,
                MonitorTriggerKind.JobFinished,
                MonitorTriggerKind.Progress,
                MonitorTriggerKind.TimeElapsed,
                MonitorTriggerKind.TimeRemaining
            };

            if (GetSelectedJobType() == JobType.MultiRun)
            {
                types.AddRange(
                [
                    MonitorTriggerKind.TestedCount,
                    MonitorTriggerKind.HitCount,
                    MonitorTriggerKind.CustomCount,
                    MonitorTriggerKind.ToCheckCount,
                    MonitorTriggerKind.FailCount,
                    MonitorTriggerKind.RetryCount,
                    MonitorTriggerKind.BanCount,
                    MonitorTriggerKind.ErrorCount,
                    MonitorTriggerKind.AliveProxiesCount,
                    MonitorTriggerKind.BannedProxiesCount,
                    MonitorTriggerKind.CpmCount,
                    MonitorTriggerKind.CaptchaCredit
                ]);
            }

            return types;
        }
    }

    public IEnumerable<MonitorActionKind> AvailableActionTypes
    {
        get
        {
            var types = new List<MonitorActionKind>
            {
                MonitorActionKind.Wait,
                MonitorActionKind.SetRelativeStartCondition,
                MonitorActionKind.StopJob,
                MonitorActionKind.AbortJob,
                MonitorActionKind.StartJob,
                MonitorActionKind.DiscordWebhook,
                MonitorActionKind.TelegramBot
            };

            if (GetSelectedJobType() == JobType.MultiRun)
            {
                types.Add(MonitorActionKind.SetBots);
                types.Add(MonitorActionKind.SetSkip);
                types.Add(MonitorActionKind.ReloadProxies);
            }

            return types;
        }
    }

    public EditTriggeredActionViewModel(IReadOnlyList<Job> jobs, TriggeredActionEditMode mode, TriggeredAction? action)
    {
        Mode = mode;
        availableJobs = jobs
            .Where(j => j is MultiRunJob or ProxyCheckJob)
            .Select(MonitorJobOption.FromJob)
            .OrderBy(j => j.Id)
            .ToList();

        AvailableJobs = new ObservableCollection<MonitorJobOption>(availableJobs);

        if (action is null)
        {
            SelectedJobId = availableJobs.FirstOrDefault()?.Id ?? -1;
            EnsureSelectedTypesAreValid();
            return;
        }

        OriginalId = action.Id;
        preserveRuntimeState = mode == TriggeredActionEditMode.Edit;
        originalExecutions = action.Executions;
        originalIsExecuting = action.IsExecuting;

        Name = mode == TriggeredActionEditMode.Edit ? action.Name : string.Empty;
        IsActive = action.IsActive;
        IsRepeatable = action.IsRepeatable;
        SelectedJobId = action.JobId;

        foreach (var trigger in action.Triggers.Select(CreateTriggerEditor))
        {
            Triggers.Add(trigger);
        }

        foreach (var monitorAction in action.Actions.Select(a => CreateActionEditor(a, availableJobs)))
        {
            Actions.Add(monitorAction);
        }

        EnsureSelectedTypesAreValid();
    }

    public void AddSelectedTrigger() => Triggers.Add(CreateTriggerEditor(SelectedTriggerType));

    public void RemoveTrigger(MonitorTriggerEditorViewModel trigger) => Triggers.Remove(trigger);

    public void AddSelectedAction() => Actions.Add(CreateActionEditor(SelectedActionType, availableJobs, SelectedJobId));

    public void RemoveAction(MonitorActionEditorViewModel action) => Actions.Remove(action);

    public bool TryValidate(out string message)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            message = "The name cannot be blank";
            return false;
        }

        if (!availableJobs.Any(j => j.Id == SelectedJobId))
        {
            message = "Please select a monitored job";
            return false;
        }

        if (Triggers.Count == 0)
        {
            message = "Please add at least one trigger";
            return false;
        }

        if (Actions.Count == 0)
        {
            message = "Please add at least one action";
            return false;
        }

        var invalidJobAction = Actions
            .OfType<IJobActionEditor>()
            .FirstOrDefault(a => !availableJobs.Any(j => j.Id == a.JobId));

        if (invalidJobAction is not null)
        {
            message = "One or more actions reference an invalid job";
            return false;
        }

        var invalidTelegramAction = Actions
            .OfType<TelegramBotActionEditorViewModel>()
            .FirstOrDefault(a => !a.IsChatIdValid);

        if (invalidTelegramAction is not null)
        {
            message = "One or more Telegram actions have an invalid chat id";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public TriggeredAction BuildTriggeredAction()
    {
        var baseAction = new TriggeredAction
        {
            Name = Name,
            IsActive = IsActive,
            IsRepeatable = IsRepeatable,
            JobId = SelectedJobId,
            Triggers = Triggers.Select(t => t.Build()).ToList(),
            Actions = Actions.Select(a => a.Build()).ToList()
        };

        if (preserveRuntimeState)
        {
            return new TriggeredAction
            {
                Id = OriginalId,
                Name = baseAction.Name,
                IsActive = baseAction.IsActive,
                IsExecuting = originalIsExecuting,
                IsRepeatable = baseAction.IsRepeatable,
                Executions = originalExecutions,
                JobId = baseAction.JobId,
                Triggers = baseAction.Triggers,
                Actions = baseAction.Actions
            };
        }

        return baseAction;
    }

    private JobType GetSelectedJobType()
        => availableJobs.FirstOrDefault(j => j.Id == SelectedJobId)?.Type ?? JobType.MultiRun;

    private void EnsureSelectedTypesAreValid()
    {
        if (!AvailableTriggerTypes.Contains(SelectedTriggerType))
        {
            SelectedTriggerType = AvailableTriggerTypes.First();
        }

        if (!AvailableActionTypes.Contains(SelectedActionType))
        {
            SelectedActionType = AvailableActionTypes.First();
        }
    }

    private static MonitorTriggerEditorViewModel CreateTriggerEditor(MonitorTrigger trigger) => trigger switch
    {
        JobStatusTrigger t => new JobStatusTriggerEditorViewModel(t.Status),
        JobFinishedTrigger => new SimpleTriggerEditorViewModel(MonitorTriggerKind.JobFinished, "The job has finished"),
        TestedCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.TestedCount, "The number of tested data lines is", t.Comparison, t.Amount),
        HitCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.HitCount, "The number of hits is", t.Comparison, t.Amount),
        CustomCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.CustomCount, "The number of custom results is", t.Comparison, t.Amount),
        ToCheckCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.ToCheckCount, "The number of results to check is", t.Comparison, t.Amount),
        FailCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.FailCount, "The number of fails is", t.Comparison, t.Amount),
        RetryCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.RetryCount, "The number of retries is", t.Comparison, t.Amount),
        BanCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.BanCount, "The number of bans is", t.Comparison, t.Amount),
        ErrorCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.ErrorCount, "The number of errors is", t.Comparison, t.Amount),
        AliveProxiesCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.AliveProxiesCount, "The number of alive proxies is", t.Comparison, t.Amount),
        BannedProxiesCountTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.BannedProxiesCount, "The number of banned proxies is", t.Comparison, t.Amount),
        CPMTrigger t => new IntComparisonTriggerEditorViewModel(MonitorTriggerKind.CpmCount, "The number of Checks Per Minute is", t.Comparison, t.Amount),
        CaptchaCreditTrigger t => new FloatComparisonTriggerEditorViewModel(MonitorTriggerKind.CaptchaCredit, "The captcha credit is", t.Comparison, t.Amount),
        ProgressTrigger t => new FloatComparisonTriggerEditorViewModel(MonitorTriggerKind.Progress, "The progress is", t.Comparison, t.Amount),
        TimeElapsedTrigger t => new TimeComparisonTriggerEditorViewModel(MonitorTriggerKind.TimeElapsed, "The elapsed time is", t.Comparison, new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)),
        TimeRemainingTrigger t => new TimeComparisonTriggerEditorViewModel(MonitorTriggerKind.TimeRemaining, "The remaining time is", t.Comparison, new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)),
        _ => throw new NotImplementedException($"Unsupported trigger type {trigger.GetType().Name}")
    };

    private static MonitorTriggerEditorViewModel CreateTriggerEditor(MonitorTriggerKind kind) => kind switch
    {
        MonitorTriggerKind.JobStatus => new JobStatusTriggerEditorViewModel(JobStatus.Running),
        MonitorTriggerKind.JobFinished => new SimpleTriggerEditorViewModel(MonitorTriggerKind.JobFinished, "The job has finished"),
        MonitorTriggerKind.TestedCount => new IntComparisonTriggerEditorViewModel(kind, "The number of tested data lines is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.HitCount => new IntComparisonTriggerEditorViewModel(kind, "The number of hits is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.CustomCount => new IntComparisonTriggerEditorViewModel(kind, "The number of custom results is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.ToCheckCount => new IntComparisonTriggerEditorViewModel(kind, "The number of results to check is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.FailCount => new IntComparisonTriggerEditorViewModel(kind, "The number of fails is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.RetryCount => new IntComparisonTriggerEditorViewModel(kind, "The number of retries is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.BanCount => new IntComparisonTriggerEditorViewModel(kind, "The number of bans is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.ErrorCount => new IntComparisonTriggerEditorViewModel(kind, "The number of errors is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.AliveProxiesCount => new IntComparisonTriggerEditorViewModel(kind, "The number of alive proxies is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.BannedProxiesCount => new IntComparisonTriggerEditorViewModel(kind, "The number of banned proxies is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.CpmCount => new IntComparisonTriggerEditorViewModel(kind, "The number of Checks Per Minute is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.CaptchaCredit => new FloatComparisonTriggerEditorViewModel(kind, "The captcha credit is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.Progress => new FloatComparisonTriggerEditorViewModel(kind, "The progress is", NumComparison.EqualTo, 0),
        MonitorTriggerKind.TimeElapsed => new TimeComparisonTriggerEditorViewModel(kind, "The elapsed time is", NumComparison.EqualTo, TimeSpan.Zero),
        MonitorTriggerKind.TimeRemaining => new TimeComparisonTriggerEditorViewModel(kind, "The remaining time is", NumComparison.EqualTo, TimeSpan.Zero),
        _ => throw new NotImplementedException()
    };

    private static MonitorActionEditorViewModel CreateActionEditor(
        MonitorAction action,
        IReadOnlyList<MonitorJobOption> jobs) => action switch
        {
            WaitAction a => new WaitActionEditorViewModel(new TimeSpan(a.Days, a.Hours, a.Minutes, a.Seconds)),
            SetRelativeStartConditionAction a => new JobAndTimeSpanActionEditorViewModel(MonitorActionKind.SetRelativeStartCondition, "Set the relative time start condition to", jobs, a.JobId, new TimeSpan(a.Days, a.Hours, a.Minutes, a.Seconds)),
            StopJobAction a => new JobTargetActionEditorViewModel(MonitorActionKind.StopJob, "Stop job", jobs, a.JobId),
            AbortJobAction a => new JobTargetActionEditorViewModel(MonitorActionKind.AbortJob, "Abort job", jobs, a.JobId),
            StartJobAction a => new JobTargetActionEditorViewModel(MonitorActionKind.StartJob, "Start job", jobs, a.JobId),
            DiscordWebhookAction a => new DiscordWebhookActionEditorViewModel(a.Webhook, a.Message),
            TelegramBotAction a => new TelegramBotActionEditorViewModel(a.Token, a.ChatId, a.Message),
            SetBotsAction a => new SetBotsActionEditorViewModel(a.Amount),
            SetSkipAction a => new SetSkipActionEditorViewModel(a.Skip),
            ReloadProxiesAction => new SimpleActionEditorViewModel(MonitorActionKind.ReloadProxies, "Reload the proxies"),
            _ => throw new NotImplementedException($"Unsupported action type {action.GetType().Name}")
        };

    private static MonitorActionEditorViewModel CreateActionEditor(
        MonitorActionKind kind,
        IReadOnlyList<MonitorJobOption> jobs,
        int selectedJobId) => kind switch
        {
            MonitorActionKind.Wait => new WaitActionEditorViewModel(TimeSpan.Zero),
            MonitorActionKind.SetRelativeStartCondition => new JobAndTimeSpanActionEditorViewModel(kind, "Set the relative time start condition to", jobs, selectedJobId, TimeSpan.Zero),
            MonitorActionKind.StopJob => new JobTargetActionEditorViewModel(kind, "Stop job", jobs, selectedJobId),
            MonitorActionKind.AbortJob => new JobTargetActionEditorViewModel(kind, "Abort job", jobs, selectedJobId),
            MonitorActionKind.StartJob => new JobTargetActionEditorViewModel(kind, "Start job", jobs, selectedJobId),
            MonitorActionKind.DiscordWebhook => new DiscordWebhookActionEditorViewModel(string.Empty, string.Empty),
            MonitorActionKind.TelegramBot => new TelegramBotActionEditorViewModel(string.Empty, 0, string.Empty),
            MonitorActionKind.SetBots => new SetBotsActionEditorViewModel(1),
            MonitorActionKind.SetSkip => new SetSkipActionEditorViewModel(0),
            MonitorActionKind.ReloadProxies => new SimpleActionEditorViewModel(MonitorActionKind.ReloadProxies, "Reload the proxies"),
            _ => throw new NotImplementedException()
        };
}

public class MonitorJobOption
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public JobType Type { get; init; }
    public string DisplayName => $"#{Id} | {Name} | {TypeText}";
    public string TypeText => Type switch
    {
        JobType.MultiRun => "Multi Run Job",
        JobType.ProxyCheck => "Proxy Check Job",
        _ => Type.ToString()
    };

    public static MonitorJobOption FromJob(Job job) => new()
    {
        Id = job.Id,
        Name = string.IsNullOrWhiteSpace(job.Name) ? "Unnamed" : job.Name,
        Type = job switch
        {
            MultiRunJob => JobType.MultiRun,
            ProxyCheckJob => JobType.ProxyCheck,
            _ => throw new NotImplementedException()
        }
    };
}

public abstract class MonitorTriggerEditorViewModel(MonitorTriggerKind kind) : ViewModelBase
{
    public MonitorTriggerKind Kind { get; } = kind;
    public abstract MonitorTrigger Build();
}

public class JobStatusTriggerEditorViewModel(JobStatus status) : MonitorTriggerEditorViewModel(MonitorTriggerKind.JobStatus)
{
    public IEnumerable<JobStatus> JobStatuses => Enum.GetValues(typeof(JobStatus)).Cast<JobStatus>();

    private JobStatus status = status;
    public JobStatus Status
    {
        get => status;
        set
        {
            status = value;
            OnPropertyChanged();
        }
    }

    public override MonitorTrigger Build() => new JobStatusTrigger { Status = Status };
}

public class SimpleTriggerEditorViewModel(MonitorTriggerKind kind, string description) : MonitorTriggerEditorViewModel(kind)
{
    public string Description { get; } = description;

    public override MonitorTrigger Build() => Kind switch
    {
        MonitorTriggerKind.JobFinished => new JobFinishedTrigger(),
        _ => throw new NotImplementedException()
    };
}

public class IntComparisonTriggerEditorViewModel(
    MonitorTriggerKind kind,
    string prefix,
    NumComparison comparison,
    int amount) : MonitorTriggerEditorViewModel(kind)
{
    public string Prefix { get; } = prefix;
    public IEnumerable<NumComparison> NumComparisons => Enum.GetValues(typeof(NumComparison)).Cast<NumComparison>();

    private NumComparison comparison = comparison;
    public NumComparison Comparison
    {
        get => comparison;
        set
        {
            comparison = value;
            OnPropertyChanged();
        }
    }

    private int amount = amount;
    public int Amount
    {
        get => amount;
        set
        {
            amount = value;
            OnPropertyChanged();
        }
    }

    public override MonitorTrigger Build() => Kind switch
    {
        MonitorTriggerKind.TestedCount => new TestedCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.HitCount => new HitCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.CustomCount => new CustomCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.ToCheckCount => new ToCheckCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.FailCount => new FailCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.RetryCount => new RetryCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.BanCount => new BanCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.ErrorCount => new ErrorCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.AliveProxiesCount => new AliveProxiesCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.BannedProxiesCount => new BannedProxiesCountTrigger { Comparison = Comparison, Amount = Amount },
        MonitorTriggerKind.CpmCount => new CPMTrigger { Comparison = Comparison, Amount = Amount },
        _ => throw new NotImplementedException()
    };
}

public class FloatComparisonTriggerEditorViewModel(
    MonitorTriggerKind kind,
    string prefix,
    NumComparison comparison,
    double amount) : MonitorTriggerEditorViewModel(kind)
{
    public string Prefix { get; } = prefix;
    public IEnumerable<NumComparison> NumComparisons => Enum.GetValues(typeof(NumComparison)).Cast<NumComparison>();

    private NumComparison comparison = comparison;
    public NumComparison Comparison
    {
        get => comparison;
        set
        {
            comparison = value;
            OnPropertyChanged();
        }
    }

    private double amount = amount;
    public double Amount
    {
        get => amount;
        set
        {
            amount = value;
            OnPropertyChanged();
        }
    }

    public override MonitorTrigger Build() => Kind switch
    {
        MonitorTriggerKind.CaptchaCredit => new CaptchaCreditTrigger { Comparison = Comparison, Amount = (float)Amount },
        MonitorTriggerKind.Progress => new ProgressTrigger { Comparison = Comparison, Amount = (float)Amount },
        _ => throw new NotImplementedException()
    };
}

public class TimeComparisonTriggerEditorViewModel(
    MonitorTriggerKind kind,
    string prefix,
    NumComparison comparison,
    TimeSpan timeSpan) : MonitorTriggerEditorViewModel(kind)
{
    public string Prefix { get; } = prefix;
    public IEnumerable<NumComparison> NumComparisons => Enum.GetValues(typeof(NumComparison)).Cast<NumComparison>();

    private NumComparison comparison = comparison;
    public NumComparison Comparison
    {
        get => comparison;
        set
        {
            comparison = value;
            OnPropertyChanged();
        }
    }

    private TimeSpan timeSpan = timeSpan;
    public TimeSpan TimeSpan
    {
        get => timeSpan;
        set
        {
            timeSpan = value;
            OnPropertyChanged();
        }
    }

    public override MonitorTrigger Build() => Kind switch
    {
        MonitorTriggerKind.TimeElapsed => new TimeElapsedTrigger
        {
            Comparison = Comparison,
            Days = TimeSpan.Days,
            Hours = TimeSpan.Hours,
            Minutes = TimeSpan.Minutes,
            Seconds = TimeSpan.Seconds
        },
        MonitorTriggerKind.TimeRemaining => new TimeRemainingTrigger
        {
            Comparison = Comparison,
            Days = TimeSpan.Days,
            Hours = TimeSpan.Hours,
            Minutes = TimeSpan.Minutes,
            Seconds = TimeSpan.Seconds
        },
        _ => throw new NotImplementedException()
    };
}

public interface IJobActionEditor
{
    int JobId { get; }
}

public abstract class MonitorActionEditorViewModel(MonitorActionKind kind) : ViewModelBase
{
    public MonitorActionKind Kind { get; } = kind;
    public abstract MonitorAction Build();
}

public class WaitActionEditorViewModel(TimeSpan timeSpan) : MonitorActionEditorViewModel(MonitorActionKind.Wait)
{
    private TimeSpan timeSpan = timeSpan;
    public TimeSpan TimeSpan
    {
        get => timeSpan;
        set
        {
            timeSpan = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new WaitAction
    {
        Days = TimeSpan.Days,
        Hours = TimeSpan.Hours,
        Minutes = TimeSpan.Minutes,
        Seconds = TimeSpan.Seconds
    };
}

public class JobAndTimeSpanActionEditorViewModel(
    MonitorActionKind kind,
    string prefix,
    IReadOnlyList<MonitorJobOption> jobs,
    int jobId,
    TimeSpan timeSpan) : MonitorActionEditorViewModel(kind), IJobActionEditor
{
    public string Prefix { get; } = prefix;
    public IReadOnlyList<MonitorJobOption> AvailableJobs { get; } = jobs;

    private int jobId = jobId;
    public int JobId
    {
        get => jobId;
        set
        {
            jobId = value;
            OnPropertyChanged();
        }
    }

    private TimeSpan timeSpan = timeSpan;
    public TimeSpan TimeSpan
    {
        get => timeSpan;
        set
        {
            timeSpan = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new SetRelativeStartConditionAction
    {
        JobId = JobId,
        Days = TimeSpan.Days,
        Hours = TimeSpan.Hours,
        Minutes = TimeSpan.Minutes,
        Seconds = TimeSpan.Seconds
    };
}

public class JobTargetActionEditorViewModel(
    MonitorActionKind kind,
    string prefix,
    IReadOnlyList<MonitorJobOption> jobs,
    int jobId) : MonitorActionEditorViewModel(kind), IJobActionEditor
{
    public string Prefix { get; } = prefix;
    public IReadOnlyList<MonitorJobOption> AvailableJobs { get; } = jobs;

    private int jobId = jobId;
    public int JobId
    {
        get => jobId;
        set
        {
            jobId = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => Kind switch
    {
        MonitorActionKind.StopJob => new StopJobAction { JobId = JobId },
        MonitorActionKind.AbortJob => new AbortJobAction { JobId = JobId },
        MonitorActionKind.StartJob => new StartJobAction { JobId = JobId },
        _ => throw new NotImplementedException()
    };
}

public class DiscordWebhookActionEditorViewModel(string webhook, string message)
    : MonitorActionEditorViewModel(MonitorActionKind.DiscordWebhook)
{
    private string webhook = webhook;
    public string Webhook
    {
        get => webhook;
        set
        {
            webhook = value;
            OnPropertyChanged();
        }
    }

    private string message = message;
    public string Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new DiscordWebhookAction
    {
        Webhook = Webhook,
        Message = Message
    };
}

public class TelegramBotActionEditorViewModel(string token, long chatId, string message)
    : MonitorActionEditorViewModel(MonitorActionKind.TelegramBot)
{
    private string token = token;
    public string Token
    {
        get => token;
        set
        {
            token = value;
            OnPropertyChanged();
        }
    }

    private string chatIdText = chatId.ToString();
    public string ChatIdText
    {
        get => chatIdText;
        set
        {
            chatIdText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsChatIdValid));
        }
    }

    public bool IsChatIdValid => long.TryParse(ChatIdText, out _);

    private string message = message;
    public string Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new TelegramBotAction
    {
        Token = Token,
        ChatId = long.TryParse(ChatIdText, out var chatId) ? chatId : 0,
        Message = Message
    };
}

public class SetBotsActionEditorViewModel(int amount) : MonitorActionEditorViewModel(MonitorActionKind.SetBots)
{
    private int amount = amount;
    public int Amount
    {
        get => amount;
        set
        {
            amount = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new SetBotsAction { Amount = Amount };
}

public class SetSkipActionEditorViewModel(int skip) : MonitorActionEditorViewModel(MonitorActionKind.SetSkip)
{
    private int skip = skip;
    public int Skip
    {
        get => skip;
        set
        {
            skip = value;
            OnPropertyChanged();
        }
    }

    public override MonitorAction Build() => new SetSkipAction { Skip = Skip };
}

public class SimpleActionEditorViewModel(MonitorActionKind kind, string description) : MonitorActionEditorViewModel(kind)
{
    public string Description { get; } = description;

    public override MonitorAction Build() => Kind switch
    {
        MonitorActionKind.ReloadProxies => new ReloadProxiesAction(),
        _ => throw new NotImplementedException()
    };
}
