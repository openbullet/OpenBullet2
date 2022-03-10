using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Logging;
using System;
using System.Collections.Generic;
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
            vm.LogCleared += ClearLog;

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

        private void TakeStep(object sender, RoutedEventArgs e) => vm.TakeStep();

        private void Stop(object sender, RoutedEventArgs e) => vm.Stop();

        private void NewLogEntry(object sender, BotLoggerEntry entry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Append the log message
                logRTB.AppendText(entry.Message + Environment.NewLine, entry.Color);

                // Scroll to the bottom of the log
                try
                {
                    logRTB.SelectionStart = logRTB.TextLength;
                    logRTB.ScrollToCaret();
                    logRTB.ClearUndoHistory();
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

                try
                {
                    logRTB.ClearUndoHistory();
                }
                catch
                {

                }

                // Update the HTML view
                if (entry.CanViewAsHtml)
                {
                    htmlViewer.HTML = entry.Message;
                }
            });
        }

        private void ClearLog(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                logRTB.Clear();
                variablesRTB.Clear();
                htmlViewer.HTML = string.Empty;
            });
        }

        #region Search
        private void Search(object sender, RoutedEventArgs e)
        {
            // Reset all highlights
            logRTB.SelectAll();
            logRTB.SelectionBackColor = System.Drawing.Color.FromArgb(22, 22, 22);
            logRTB.DeselectAll();

            // Check for empty search
            if (string.IsNullOrWhiteSpace(vm.SearchString))
            {
                return;
            }

            var selectionStart = logRTB.SelectionStart;
            var startIndex = 0;
            var indices = new List<int>();
            int index;

            while ((index = logRTB.Text.IndexOf(vm.SearchString, startIndex, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                logRTB.Select(index, vm.SearchString.Length);
                logRTB.SelectionColor = System.Drawing.Color.White;
                logRTB.SelectionBackColor = System.Drawing.Color.Navy;

                startIndex = index + vm.SearchString.Length;
                indices.Add(startIndex);

                // If it's the first match, immediately scroll to it
                if (indices.Count == 1)
                {
                    logRTB.ScrollToCaret();
                }
            }

            vm.Indices = indices.ToArray();

            // Reset the selection
            logRTB.SelectionStart = selectionStart;
            logRTB.SelectionLength = 0;
            logRTB.SelectionColor = System.Drawing.Color.Black;
        }

        private void PreviousMatch(object sender, RoutedEventArgs e)
        {
            // If no matches, do nothing
            if (vm.Indices.Length == 0)
            {
                return;
            }

            // If we need to loop around
            if (vm.CurrentMatchIndex == 0)
            {
                vm.CurrentMatchIndex = vm.Indices.Length - 1;
            }
            else
            {
                vm.CurrentMatchIndex--;
            }

            logRTB.DeselectAll();
            logRTB.Select(vm.Indices[vm.CurrentMatchIndex], 0);
            logRTB.ScrollToCaret();
        }

        private void NextMatch(object sender, RoutedEventArgs e)
        {
            // If no matches, do nothing
            if (vm.Indices.Length == 0)
            {
                return;
            }

            // If we need to loop around
            if (vm.CurrentMatchIndex == vm.Indices.Length - 1)
            {
                vm.CurrentMatchIndex = 0;
            }
            else
            {
                vm.CurrentMatchIndex++;
            }

            logRTB.DeselectAll();
            logRTB.Select(vm.Indices[vm.CurrentMatchIndex], 0);
            logRTB.ScrollToCaret();
        }
        #endregion
    }
}
