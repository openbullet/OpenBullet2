using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for TimeSpanPicker.xaml
/// </summary>
public partial class TimeSpanPicker : UserControl
{
    private bool updatingControls;

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
        new FrameworkPropertyMetadata(
            default(TimeSpan),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnTimeSpanPropertyChanged));

    private static void OnTimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var newValue = (TimeSpan)e.NewValue;
        if (d is not TimeSpanPicker source)
        {
            return;
        }

        source.updatingControls = true;
        try
        {
            source.hours.Value = (int)newValue.TotalHours;
            source.minutes.Value = newValue.Minutes;
            source.seconds.Value = newValue.Seconds;
        }
        finally
        {
            source.updatingControls = false;
        }
    }

    public TimeSpanPicker()
    {
        InitializeComponent();
    }

    private void NumberChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
    {
        if (updatingControls)
        {
            return;
        }

        if (hours?.Value is double hoursValue
            && minutes?.Value is double minutesValue
            && seconds?.Value is double secondsValue)
        {
            SetCurrentValue(TimeSpanProperty, new TimeSpan((int)hoursValue, (int)minutesValue, (int)secondsValue));
            GetBindingExpression(TimeSpanProperty)?.UpdateSource();
        }
    }
}
