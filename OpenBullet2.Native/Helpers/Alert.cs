using System;
using System.Windows;

namespace OpenBullet2.Native.Helpers
{
    public static class Alert
    {
        // TODO: Style the alerts
        public static void Info(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        public static void Warning(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        public static void Error(string title, string message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        public static bool Choice(string title, string message) 
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        
        public static void Exception(Exception ex)  => Error(ex.GetType().Name, ex.Message);
    }
}
