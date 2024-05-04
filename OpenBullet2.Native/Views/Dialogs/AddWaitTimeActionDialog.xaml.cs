using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs.Monitor.Actions;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddWaitTimeActionDialog.xaml
    /// </summary>
    public partial class AddWaitTimeActionDialog : Page
    {
        private TriggeredActionViewModel vm;
        private WaitAction action;
        public AddWaitTimeActionDialog(TriggeredActionViewModel vm)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
        }
        public AddWaitTimeActionDialog(TriggeredActionViewModel vm, WaitAction action)
        {
            this.vm = vm;
            DataContext = this.vm;
            InitializeComponent();
            this.action = action;
            daysTextBox.Value = action.Days;
            hoursTextBox.Value = action.Hours;
            minutesTextBox.Value = action.Minutes;
            secondsTextBox.Value = action.Seconds;
        }

        private void Accept(object sender, RoutedEventArgs e)
        {

            if (daysTextBox?.Value == null || daysTextBox?.Value == null || daysTextBox?.Value == null || daysTextBox?.Value == null)
            {
                Alert.Error("Invalid input", "Check your values and try again");
                return;
            }

            if (action is not null)
            {
                action.Days = (int)daysTextBox.Value;
                action.Hours = (int)hoursTextBox.Value;
                action.Minutes = (int)minutesTextBox.Value;
                action.Seconds = (int)secondsTextBox.Value;
            }
            else
            {
                vm.AddAction(new WaitAction()
                {
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
