using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for TimeSpanPicker.xaml
    /// </summary>
    public partial class TimeSpanPicker : UserControl
    {
        public TimeSpan TimeSpan
        {
            get => (TimeSpan)GetValue(TimeSpanProperty);
            set => SetValue(TimeSpanProperty, value);
        }

        public static readonly DependencyProperty TimeSpanProperty =
        DependencyProperty.Register(
            nameof(TimeSpan),
            typeof(TimeSpan),
            typeof(TimeSpanPicker),
            new PropertyMetadata(default(TimeSpan), OnTimeSpanPropertyChanged));

        private static void OnTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (TimeSpan)e.NewValue;
            var source = d as TimeSpanPicker;

            source.hours.Value = newValue.Hours;
            source.minutes.Value = newValue.Minutes;
            source.seconds.Value = newValue.Seconds;
        }

        public TimeSpanPicker()
        {
            InitializeComponent();
        }

        private void NumberChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (hours is not null && minutes is not null && seconds is not null)
            {
                TimeSpan = new TimeSpan((int)hours.Value, (int)minutes.Value, (int)seconds.Value);
            }
        }
    }
}
