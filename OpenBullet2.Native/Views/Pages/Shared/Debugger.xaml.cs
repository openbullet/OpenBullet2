using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Logging;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages.Shared
{
    /// <summary>
    /// Interaction logic for Debugger.xaml
    /// </summary>
    public partial class Debugger : Page
    {
        private readonly DebuggerViewModel vm;

        public Debugger()
        {
            vm = SP.GetService<ViewModelsService>().Debugger;
            DataContext = vm;

            vm.NewLogEntry += NewLogEntry;

            InitializeComponent();
            tabControl.SelectedIndex = 0;

            logRTB.Font = new System.Drawing.Font("Consolas", 10);
            logRTB.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);

            variablesRTB.Font = new System.Drawing.Font("Consolas", 10);
            variablesRTB.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);
        }

        private void ShowLog(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 0;
        private void ShowVariables(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 1;
        private void ShowHTML(object sender, RoutedEventArgs e) => tabControl.SelectedIndex = 2;

        private async void Start(object sender, RoutedEventArgs e)
        {
            if (!vm.PersistLog)
            {
                logRTB.Clear();
            }

            try
            {
                await vm.Run();
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }

        private void Stop(object sender, RoutedEventArgs e) => vm.Stop();

        private void NewLogEntry(object sender, BotLoggerEntry entry)
        {
            // Append the log message
            logRTB.AppendText(entry.Message + Environment.NewLine, entry.Color);

            // Scroll to the bottom of the log
            try
            {
                logRTB.SelectionStart = logRTB.TextLength;
                logRTB.ScrollToCaret();
            }
            catch 
            {

            }

            // Recreate the variables list
            variablesRTB.Clear();
            foreach (var variable in vm.Variables)
            {
                var color = variable.MarkedForCapture ? LogColors.Tomato : LogColors.Yellow;
                variablesRTB.AppendText($"{variable.Name} ({variable.Type}) = {variable.AsString()}", color);
            }

            // Update the HTML view
            if (entry.CanViewAsHtml)
            {
                htmlViewer.HTML = entry.Message;
            }
        }
    }
}
