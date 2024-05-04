using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels.Helpers;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Helpers;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace OpenBullet2.Native.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        private readonly JobMonitorService jobMonitorService;
        private readonly Timer secondsTicker;

        private ObservableCollection<TriggeredActionViewModel> triggeredActionCollection;
        public ObservableCollection<TriggeredActionViewModel> TriggeredActionCollection
        {
            get => triggeredActionCollection;
            set
            {
                triggeredActionCollection = value;
                OnPropertyChanged();
            }
        }

        public MonitorViewModel()
        {
            jobMonitorService = SP.GetService<JobMonitorService>();
            CreateCollection();

            secondsTicker = new Timer(new TimerCallback(_ => RefreshTriggeredActions()), null, 500, 500);
        }
        private void CreateCollection()
        {
            TriggeredActionCollection = new ObservableCollection<TriggeredActionViewModel>(jobMonitorService.TriggeredActions.Select(ta => MakeViewModel(ta)));
        }

        private void RefreshTriggeredActions()
        {
            foreach (var ta in TriggeredActionCollection)
            {
                ta.UpdateViewModel();
            }
        }

        public void CreateTriggeredAction()
        {
            var ta = new TriggeredAction
            {
                IsActive = false
            };

            jobMonitorService.TriggeredActions.Add(ta);
            TriggeredActionCollection.Add(MakeViewModel(ta));
        }

        public void AddTriggeredAction(TriggeredAction newTr)
        {
            jobMonitorService.TriggeredActions.Add(newTr);
            TriggeredActionCollection.Add(MakeViewModel(newTr));
        }
        public void RemoveTriggeredAction(TriggeredActionViewModel triggeredAction)
        {
            jobMonitorService.TriggeredActions.Remove(triggeredAction.TriggeredAction);
            TriggeredActionCollection.Remove(triggeredAction);
        }

        public TriggeredAction CloneTriggeredAction(TriggeredActionViewModel tr)
        {
            var newAction = Cloner.Clone(tr.TriggeredAction);
            newAction.IsActive = false;
            newAction.IsRepeatable = false;
            newAction.Reset();

            return newAction;
        }

        public void RemoveAll()
        {
            var executingActions = jobMonitorService.TriggeredActions.Where(ta => ta.IsExecuting);

            if (executingActions.Any())
            {
                throw new System.Exception($"A monitor for job #{executingActions.First().JobId} is still executing, wait for it to finish or reset");
            }

            jobMonitorService.TriggeredActions.Clear();
            TriggeredActionCollection.Clear();
        }

        private static TriggeredActionViewModel MakeViewModel(TriggeredAction ta) => new TriggeredActionViewModel(ta);
    }

    public class TriggeredActionViewModel : ViewModelBase
    {
        public TriggeredAction TriggeredAction { get; init; }
        public bool IsActive 
        {
            get => TriggeredAction.IsActive;
            set
            {
                TriggeredAction.IsActive = value;
                OnPropertyChanged();
            }
        }
        public bool IsExecuting
        {
            get => TriggeredAction.IsExecuting;
        }
        public bool IsRepeatable
        {
            get => TriggeredAction.IsRepeatable;
            set
            {
                TriggeredAction.IsRepeatable = value;
                OnPropertyChanged();
            }
        }
        public int Executions => TriggeredAction.Executions;
        public int JobId
        {
            get => TriggeredAction.JobId;
            set
            {
                TriggeredAction.JobId = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Trigger> triggersCollection;
        public ObservableCollection<Trigger> TriggersCollection
        {
            get => triggersCollection;
            set
            {
                triggersCollection = value;
                OnPropertyChanged();
            }
        }
        public bool IsTriggersEmpty => TriggersCollection.Count == 0;
        public ICommand RemoveTriggerCommand { get; }
        public ICommand EditTriggerCommand { get; }

        private ObservableCollection<Action> actionsCollection;
        public ObservableCollection<Action> ActionsCollection
        {
            get => actionsCollection;
            set
            {
                actionsCollection = value;
                OnPropertyChanged();
            }
        }
        public bool IsActionsEmpty => ActionsCollection.Count == 0;

        public ICommand RemoveActionCommand { get; }
        public ICommand EditActionCommand { get; }


        public TriggeredActionViewModel(TriggeredAction triggeredAction)
        {
            TriggeredAction = triggeredAction;

            triggersCollection = new ObservableCollection<Trigger>(TriggeredAction.Triggers);
            actionsCollection = new ObservableCollection<Action>(TriggeredAction.Actions);

            RemoveTriggerCommand = new RelayCommand<Trigger>(RemoveTrigger);
            EditTriggerCommand = new RelayCommand<Trigger>(EditTrigger);

            RemoveActionCommand = new RelayCommand<Action>(RemoveAction);
            EditActionCommand = new RelayCommand<Action>(EditAction);
        }
        public void AddTrigger(Trigger trigger)
        {
            TriggeredAction.Triggers.Add(trigger);
            TriggersCollection.Add(trigger);
            OnPropertyChanged(nameof(IsTriggersEmpty));
        }
        private void RemoveTrigger(Trigger trigger)
        {
            if (TriggeredAction.Triggers.Contains(trigger))
            {
                TriggeredAction.Triggers.Remove(trigger);
                TriggersCollection.Remove(trigger);
                OnPropertyChanged(nameof(IsTriggersEmpty));
            }
        }
        private void EditTrigger(Trigger trigger)
        {
            if (trigger is TimeElapsedTrigger timeElapsedTrigger)
            {
                new MainDialog(new AddTimeElapsedTriggerDialog(this, timeElapsedTrigger), "Edit Time Elapsed Trigger").ShowDialog();
                RefreshTriggersCollection();
            }
            else
                throw new System.NotImplementedException($"Editing not implemented for trigger type: {trigger.GetType().Name}");
        }
        private void RefreshTriggersCollection()
        {
            TriggersCollection = new ObservableCollection<Trigger>(triggersCollection);
        }
        
        public void AddAction(Action action)
        {
            TriggeredAction.Actions.Add(action);
            ActionsCollection.Add(action);
            OnPropertyChanged(nameof(IsActionsEmpty));
        }

        private void RemoveAction(Action action)
        {
            if (TriggeredAction.Actions.Contains(action))
            {
                TriggeredAction.Actions.Remove(action);
                ActionsCollection.Remove(action);
                UpdateViewModel();
            }
        }

        private void EditAction(Action action)
        {
            if (action is WaitAction waitAction)
            {
                new MainDialog(new AddWaitTimeActionDialog(this, waitAction), "Edit Wait Action").ShowDialog();
                RefreshActionsCollection();
            }
            else if(action is TelegramBotAction telegramAction)
            {
                new MainDialog(new AddWebhookActionDialog(this, telegramAction), "Edit Telegram Webhook Action").ShowDialog();
                RefreshActionsCollection();
            }
            else if (action is DiscordWebhookAction discordAction)
            {
                new MainDialog(new AddWebhookActionDialog(this, discordAction), "Edit Discord Webhook Action").ShowDialog();
                RefreshActionsCollection();
            }
            else
                throw new System.NotImplementedException($"Editing not implemented for actions type: {action.GetType().Name}");
        }
        private void RefreshActionsCollection()
        {
            ActionsCollection = new ObservableCollection<Action>(actionsCollection);
        }

        public void Reset()
        {
            TriggeredAction.Reset();
            UpdateViewModel();
        }

        public static IEnumerable<JobStatus> JobStatuses => System.Enum.GetValues(typeof(JobStatus)).Cast<JobStatus>();
        public static IEnumerable<NumComparison> NumComparisons => System.Enum.GetValues(typeof(NumComparison)).Cast<NumComparison>();

    }


}
