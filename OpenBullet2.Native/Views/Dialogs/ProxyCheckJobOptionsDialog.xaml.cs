using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for ProxyCheckJobOptionsDialog.xaml
/// </summary>
public partial class ProxyCheckJobOptionsDialog : Page
{
    private readonly Func<JobOptions, Task>? onAccept;
    private readonly ProxyCheckJobOptionsViewModel vm;

    public ProxyCheckJobOptionsDialog(
        ProxyCheckJobOptionsViewModel vm,
        Func<JobOptions, Task>? onAccept)
        : this(vm, null, onAccept)
    {
    }

    public ProxyCheckJobOptionsDialog(
        ProxyCheckJobOptionsViewModel vm,
        ProxyCheckJobOptions? options,
        Func<JobOptions, Task>? onAccept)
    {
        this.onAccept = onAccept;
        this.vm = vm;
        vm.Initialize(options);
        DataContext = vm;

        vm.StartConditionModeChanged += mode => startConditionTabControl.SelectedIndex = (int)mode;

        InitializeComponent();

        startConditionTabControl.SelectedIndex = (int)vm.StartConditionMode;
    }

    private async void Accept(object sender, RoutedEventArgs e)
    {
        try
        {
            if (onAccept is not null)
            {
                await onAccept(vm.Options);
            }

            ((MainDialog)Parent).Close();
        }
        catch (Exception ex)
        {
            Alert.Exception(ex);
        }
    }
}

public class ProxyCheckJobOptionsViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly JobFactoryService jobFactory;
    private readonly OpenBulletSettingsService obSettingsService;
    public ProxyCheckJobOptions Options { get; private set; }

    #region Start Condition
    public event Action<StartConditionMode>? StartConditionModeChanged;

    public StartConditionMode StartConditionMode
    {
        get => Options.StartCondition switch
        {
            RelativeTimeStartCondition => StartConditionMode.Relative,
            AbsoluteTimeStartCondition => StartConditionMode.Absolute,
            _ => throw new NotImplementedException()
        };
        set
        {
            Options.StartCondition = value switch
            {
                StartConditionMode.Relative => new RelativeTimeStartCondition(),
                StartConditionMode.Absolute => new AbsoluteTimeStartCondition(),
                _ => throw new NotImplementedException()
            };

            OnPropertyChanged();
            OnPropertyChanged(nameof(StartInMode));
            OnPropertyChanged(nameof(StartAtMode));
            StartConditionModeChanged?.Invoke(StartConditionMode);
        }
    }

    public bool StartInMode
    {
        get => StartConditionMode is StartConditionMode.Relative;
        set
        {
            if (value)
            {
                StartConditionMode = StartConditionMode.Relative;
            }

            OnPropertyChanged();
        }
    }

    public bool StartAtMode
    {
        get => StartConditionMode is StartConditionMode.Absolute;
        set
        {
            if (value)
            {
                StartConditionMode = StartConditionMode.Absolute;
            }

            OnPropertyChanged();
        }
    }

    public DateTime StartAtTime
    {
        get => Options.StartCondition is AbsoluteTimeStartCondition abs ? abs.StartAt : DateTime.Now;
        set
        {
            if (Options.StartCondition is AbsoluteTimeStartCondition abs)
            {
                abs.StartAt = value;
            }

            OnPropertyChanged();
        }
    }

    public TimeSpan StartIn
    {
        get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter : TimeSpan.Zero;
        set
        {
            if (Options.StartCondition is RelativeTimeStartCondition rel)
            {
                rel.StartAfter = value;
            }

            OnPropertyChanged();
        }
    }
    #endregion

    public ProxyCheckJobOptionsViewModel(
        IServiceScopeFactory scopeFactory,
        JobFactoryService jobFactory,
        OpenBulletSettingsService obSettingsService)
    {
        this.scopeFactory = scopeFactory;
        this.jobFactory = jobFactory;
        this.obSettingsService = obSettingsService;
        Options = null!;
    }

    public void Initialize(ProxyCheckJobOptions? options)
    {
        Options = options ?? (JobOptionsFactory.CreateNew(JobType.ProxyCheck) as ProxyCheckJobOptions
            ?? throw new InvalidOperationException("Failed to create proxy check job options"));

        using var scope = scopeFactory.CreateScope();
        proxyGroups = scope.ServiceProvider.GetRequiredService<IProxyGroupRepository>().GetAll().ToList();

        var proxyCheckTargets = obSettingsService.Settings.GeneralSettings.ProxyCheckTargets;
        Targets = proxyCheckTargets.Any()
            ? proxyCheckTargets
            : new ProxyCheckTarget[] { new() };

        // TODO: Move this to the factory!
        Target ??= Targets.FirstOrDefault() ?? new ProxyCheckTarget();
    }

    private IEnumerable<ProxyGroupEntity> proxyGroups = [];
    public IEnumerable<string> ProxyGroupNames => new[] { "All" }.Concat(proxyGroups.Select(g => g.Name ?? string.Empty));

    public string ProxyGroup
    {
        get => Options.GroupId == -1 ? "All" : proxyGroups.First(g => g.Id == Options.GroupId).Name ?? "All";
        set
        {
            Options.GroupId = value == "All" ? -1 : proxyGroups.First(g => g.Name == value).Id;
            OnPropertyChanged();
        }
    }

    public int BotLimit => jobFactory.BotLimit;

    public int Bots
    {
        get => Options.Bots;
        set
        {
            Options.Bots = value;
            OnPropertyChanged();
        }
    }

    public int TimeoutMilliseconds
    {
        get => Options.TimeoutMilliseconds;
        set
        {
            Options.TimeoutMilliseconds = value;
            OnPropertyChanged();
        }
    }

    public bool CheckOnlyUntested
    {
        get => Options.CheckOnlyUntested;
        set
        {
            Options.CheckOnlyUntested = value;
            OnPropertyChanged();
        }
    }

    public bool UseProxyJudge
    {
        get => Options.UseProxyJudge;
        set
        {
            Options.UseProxyJudge = value;
            OnPropertyChanged();
        }
    }

    private List<ProxyCheckTarget> targets = [];
    public IEnumerable<ProxyCheckTarget> Targets
    {
        get => targets;
        set
        {
            targets = value.ToList(); // Clone the list
            OnPropertyChanged();
        }
    }

    public ProxyCheckTarget Target
    {
        get => Options.Target;
        set
        {
            Options.Target = value;
            OnPropertyChanged();
        }
    }
}
