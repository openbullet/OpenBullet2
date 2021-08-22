using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ProxyCheckJobOptionsDialog.xaml
    /// </summary>
    public partial class ProxyCheckJobOptionsDialog : Page
    {
        private readonly Action<JobOptions> onAccept;
        private readonly ProxyCheckJobOptionsViewModel vm;

        public ProxyCheckJobOptionsDialog(ProxyCheckJobOptions options = null, Action<JobOptions> onAccept = null)
        {
            this.onAccept = onAccept;
            vm = new ProxyCheckJobOptionsViewModel(options);
            DataContext = vm;

            vm.StartConditionModeChanged += mode => startConditionTabControl.SelectedIndex = (int)mode;

            InitializeComponent();

            startConditionTabControl.SelectedIndex = (int)vm.StartConditionMode;
        }

        private void Accept(object sender, RoutedEventArgs e)
        {
            onAccept?.Invoke(vm.Options);
            ((MainDialog)Parent).Close();
        }
    }

    public class ProxyCheckJobOptionsViewModel : ViewModelBase
    {
        private readonly IProxyGroupRepository proxyGroupRepo;
        private readonly JobFactoryService jobFactory;
        private readonly OpenBulletSettingsService obSettingsService;
        public ProxyCheckJobOptions Options { get; init; }

        #region Start Condition
        public event Action<StartConditionMode> StartConditionModeChanged;

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

        public ProxyCheckJobOptionsViewModel(ProxyCheckJobOptions options)
        {
            Options = options ?? JobOptionsFactory.CreateNew(JobType.ProxyCheck) as ProxyCheckJobOptions;
            proxyGroupRepo = SP.GetService<IProxyGroupRepository>();
            jobFactory = SP.GetService<JobFactoryService>();
            obSettingsService = SP.GetService<OpenBulletSettingsService>();

            proxyGroups = proxyGroupRepo.GetAll().ToList();

            var proxyCheckTargets = obSettingsService.Settings.GeneralSettings.ProxyCheckTargets;
            Targets = proxyCheckTargets.Any()
                ? proxyCheckTargets
                : new ProxyCheckTarget[] { new() };

            // TODO: Move this to the factory!
            if (Target is null)
            {
                Target = Targets.FirstOrDefault();
            }
        }

        private readonly IEnumerable<ProxyGroupEntity> proxyGroups;
        public IEnumerable<string> ProxyGroupNames => new string[] { "All" }.Concat(proxyGroups.Select(g => g.Name));

        public string ProxyGroup
        {
            get => Options.GroupId == -1 ? "All" : proxyGroups.First(g => g.Id == Options.GroupId).Name;
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

        private List<ProxyCheckTarget> targets;
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
}
