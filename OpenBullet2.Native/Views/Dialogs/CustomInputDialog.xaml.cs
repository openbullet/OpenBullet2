using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for CustomInputDialog.xaml
    /// </summary>
    public partial class CustomInputDialog : Page
    {
        private readonly Action<string> onAnswer;

        public CustomInputDialog(string question, string defaultAnswer, Action<string> onAnswer)
        {
            this.onAnswer = onAnswer;
            InitializeComponent();

            this.question.Text = question;
            answerTextBox.Text = defaultAnswer;

            answerTextBox.Focus();
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            onAnswer(answerTextBox.Text);
            ((MainDialog)Parent).Close();
        }
    }
}
