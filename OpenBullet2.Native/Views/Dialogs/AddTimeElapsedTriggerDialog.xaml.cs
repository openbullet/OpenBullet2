using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddTimeElapsedTrigger.xaml
    /// </summary>
    public partial class AddTimeElapsedTriggerDialog : Page
    {
        private TriggeredActionViewModel vm;
        private TimeElapsedTrigger trigger;
        public AddTimeElapsedTriggerDialog(TriggeredActionViewModel vm)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
        }
        public AddTimeElapsedTriggerDialog(TriggeredActionViewModel vm, TimeElapsedTrigger trigger)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
            this.trigger = trigger;
            comparisonBox.SelectedItem = trigger.Comparison;
            daysTextBox.Value = trigger.Days;
            hoursTextBox.Value = trigger.Hours;
            minutesTextBox.Value = trigger.Minutes;
            secondsTextBox.Value = trigger.Seconds;
        }

        private void Accept(object sender, RoutedEventArgs e)
        {

            if (daysTextBox?.Value == null || daysTextBox?.Value == null || daysTextBox?.Value == null || daysTextBox?.Value == null)
            {
                Alert.Error("Invalid input", "Check your values and try again");
                return;
            }

            if(trigger is not null)
            {
                trigger.Comparison = (NumComparison)comparisonBox.SelectedItem;
                trigger.Days = (int)daysTextBox.Value;
                trigger.Hours = (int)hoursTextBox.Value;
                trigger.Minutes = (int)minutesTextBox.Value;
                trigger.Seconds = (int)secondsTextBox.Value;
            }
            else
            {
                vm.AddTrigger(new TimeElapsedTrigger()
                {
                    Comparison = (NumComparison)comparisonBox.SelectedItem,
                    Days = (int)daysTextBox.Value,
                    Hours = (int)hoursTextBox.Value,
                    Minutes = (int)minutesTextBox.Value,
                    Seconds = (int)secondsTextBox.Value,
                });
            }
            

            ((MainDialog)Parent).Close();
        }
    }
}
