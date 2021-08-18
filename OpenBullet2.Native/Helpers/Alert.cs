using OpenBullet2.Native.Views.Dialogs;
using System;

namespace OpenBullet2.Native.Helpers
{
    public static class Alert
    {
        public static void Info(string title, string message) => ShowAlert(AlertType.Info, title, message);
        public static void Success(string title, string message) => ShowAlert(AlertType.Success, title, message);
        public static void Warning(string title, string message) => ShowAlert(AlertType.Warning, title, message);
        public static void Error(string title, string message) => ShowAlert(AlertType.Error, title, message);
        
        public static bool Choice(string title, string message, string yesText = "Yes", string noText = "No")
        {
            var choice = false;
            new MainDialog(new ChoiceDialog(title, message, b => choice = b, yesText, noText), title).ShowDialog();
            return choice;
        }

        private static void ShowAlert(AlertType type, string title, string message)
            => new MainDialog(new AlertDialog(type, title, message), title).ShowDialog();

        public static void Exception(Exception ex)  => Error(ex.GetType().Name, ex.Message);
    }
}
