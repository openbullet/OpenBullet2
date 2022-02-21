using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace OpenBullet2.Native.Converters
{
    // From https://stackoverflow.com/a/5182660/4332314
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }
        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? True : False;

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is T convertedValue && EqualityComparer<T>.Default.Equals(convertedValue, True);
    }

    public sealed class BoolToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BoolToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        {

        }
    }

    public sealed class BoolToThicknessConverter : BooleanConverter<Thickness>
    {
        public BoolToThicknessConverter() :
            base(new Thickness(1), new Thickness(0))
        {

        }
    }

    public sealed class BoolToTextWrappingConverter : BooleanConverter<TextWrapping>
    {
        public BoolToTextWrappingConverter() :
            base(TextWrapping.Wrap, TextWrapping.NoWrap)
        {

        }
    }
}
