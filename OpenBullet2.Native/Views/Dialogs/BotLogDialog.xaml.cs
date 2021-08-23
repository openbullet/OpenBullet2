using OpenBullet2.Native.Extensions;
using RuriLib.Logging;
using System;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for BotLogDialog.xaml
    /// </summary>
    public partial class BotLogDialog : Page
    {
        public BotLogDialog(IBotLogger logger)
        {
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
            }
            catch
            {

            }
        }
    }
}
