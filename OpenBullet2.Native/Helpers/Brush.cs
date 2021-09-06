using System.Windows;
using System.Windows.Media;

namespace OpenBullet2.Native.Helpers
{
    public static class Brush
    {
        public static Color GetColor(string propertyName)
        {
            try
            {
                return ((SolidColorBrush)Application.Current.Resources[propertyName]).Color; 
            }
            catch
            {
                return ((SolidColorBrush)Application.Current.Resources["ForegroundMain"]).Color;
            }
        }

        public static SolidColorBrush Get(string propertyName)
        {
            try
            {
                return (SolidColorBrush)Application.Current.Resources[propertyName];
            }
            catch
            {
                return (SolidColorBrush)Application.Current.Resources["ForegroundMain"];
            }
        }

        public static SolidColorBrush FromHex(string hex)
            => new((Color)ColorConverter.ConvertFromString(hex));

        public static void SetAppColor(string resourceName, string color)
            => Application.Current.Resources[resourceName] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    }
}
