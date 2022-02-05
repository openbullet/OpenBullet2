using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.ViewModels;
using RuriLib.Logging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for BotLogDialog.xaml
    /// </summary>
    public partial class BotLogDialog : Page
    {
        private readonly BotLogDialogViewModel vm;

        public BotLogDialog(IBotLogger logger)
        {
            vm = new BotLogDialogViewModel();
            DataContext = vm;

            InitializeComponent();

            logRTB.Font = new System.Drawing.Font("Consolas", 10);
            logRTB.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);

            if (logger is null)
            {
                logRTB.AppendText("Bot log was not enabled when this hit was obtained", LogColors.Tomato);
                return;
            }

            foreach (var entry in logger.Entries)
            {
                // Append the log message
                logRTB.AppendText(entry.Message + Environment.NewLine, entry.Color);
            }

            try
            {
                logRTB.SelectionStart = logRTB.TextLength;
                logRTB.ScrollToCaret();
                logRTB.ClearUndoHistory();
            }
            catch
            {

            }
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

    public class BotLogDialogViewModel : ViewModelBase
    {
        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;
                OnPropertyChanged();
            }
        }

        private int[] indices = Array.Empty<int>();
        public int[] Indices
        {
            get => indices;
            set
            {
                indices = value;
                CurrentMatchIndex = 0;
            }
        }

        private int currentMatchIndex;
        public int CurrentMatchIndex
        {
            get => currentMatchIndex;
            set
            {
                currentMatchIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MatchInfo));
            }
        }

        public string MatchInfo => $"{CurrentMatchIndex + 1} of {Indices.Length}";
    }
}
