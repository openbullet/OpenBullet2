using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs.StartConditions;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for MultiRunJobOptionsDialog.xaml
    /// </summary>
    public partial class MultiRunJobOptionsDialog : Page
    {
        private readonly Action<JobOptions> onAccept;
        private readonly MultiRunJobOptionsViewModel vm;

        public MultiRunJobOptionsDialog(MultiRunJobOptions options = null, Action<JobOptions> onAccept = null)
        {
            this.onAccept = onAccept;
            vm = new MultiRunJobOptionsViewModel(options);
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

    public class MultiRunJobOptionsViewModel : ViewModelBase
    {
        public MultiRunJobOptions Options { get; init; }

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

        public int StartInHours
        {
            get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter.Hours : 0;
            set
            {
                if (Options.StartCondition is RelativeTimeStartCondition rel)
                {
                    rel.StartAfter = new TimeSpan(value, StartInMinutes, StartInSeconds);
                }

                OnPropertyChanged();
            }
        }

        public int StartInMinutes
        {
            get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter.Minutes : 0;
            set
            {
                if (Options.StartCondition is RelativeTimeStartCondition rel)
                {
                    rel.StartAfter = new TimeSpan(StartInHours, value, StartInSeconds);
                }

                OnPropertyChanged();
            }
        }

        public int StartInSeconds
        {
            get => Options.StartCondition is RelativeTimeStartCondition rel ? rel.StartAfter.Seconds : 0;
            set
            {
                if (Options.StartCondition is RelativeTimeStartCondition rel)
                {
                    rel.StartAfter = new TimeSpan(StartInHours, StartInMinutes, value);
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

        public MultiRunJobOptionsViewModel(MultiRunJobOptions options)
        {
            Options = options ?? new MultiRunJobOptions();
        }
    }

    public enum StartConditionMode
    {
        Relative,
        Absolute
    }
}
