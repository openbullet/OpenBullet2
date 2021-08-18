using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ChoiceDialog.xaml
    /// </summary>
    public partial class ChoiceDialog : Page
    {
        private readonly Action<bool> onChoice;

        public ChoiceDialog(string title, string message, Action<bool> onChoice,
            string yesText = "Yes", string noText = "No")
        {
            this.onChoice = onChoice;
            InitializeComponent();

            this.title.Text = title;
            this.message.Text = message;
            yesButtonText.Text = yesText;
            noButtonText.Text = noText;

            yesButton.Focus();
        }

        private void Yes(object sender, RoutedEventArgs e)
        {
            onChoice(true);
            ((MainDialog)Parent).Close();
        }

        private void No(object sender, RoutedEventArgs e)
        {
            onChoice(false);
            ((MainDialog)Parent).Close();
        }
    }
}
