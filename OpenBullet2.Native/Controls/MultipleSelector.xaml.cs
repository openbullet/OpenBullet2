using OpenBullet2.Native.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for MultipleSelector.xaml
    /// </summary>
    public partial class MultipleSelector : UserControl
    {
        public ObservableCollection<string> SelectedValues
        {
            get => (ObservableCollection<string>)GetValue(SelectedValuesProperty);
            set => SetValue(SelectedValuesProperty, value);
        }

        public static readonly DependencyProperty SelectedValuesProperty =
        DependencyProperty.Register(
            nameof(SelectedValues),
            typeof(ObservableCollection<string>),
            typeof(MultipleSelector),
            new PropertyMetadata(default(string), OnSelectedValuesPropertyChanged));

        private static void OnSelectedValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public ObservableCollection<string> NotSelectedValues
        {
            get => (ObservableCollection<string>)GetValue(NotSelectedValuesProperty);
            set => SetValue(NotSelectedValuesProperty, value);
        }

        public static readonly DependencyProperty NotSelectedValuesProperty =
        DependencyProperty.Register(
            nameof(NotSelectedValues),
            typeof(ObservableCollection<string>),
            typeof(MultipleSelector),
            new PropertyMetadata(default(string), OnNotSelectedValuesPropertyChanged));

        private static void OnNotSelectedValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public MultipleSelector()
        {
            InitializeComponent();
        }

        private void MoveAllRight(object sender, RoutedEventArgs e)
        {
            foreach (var value in SelectedValues)
            {
                NotSelectedValues.Add(value);
            }

            SelectedValues.Clear();
        }

        private void MoveAllLeft(object sender, RoutedEventArgs e)
        {
            foreach (var value in NotSelectedValues)
            {
                SelectedValues.Add(value);
            }

            NotSelectedValues.Clear();
        }

        private void MoveItem(object sender, RoutedEventArgs e)
        {
            var value = (sender as Label).Content as string;
            
            if (SelectedValues.Contains(value))
            {
                SelectedValues.Remove(value);
                NotSelectedValues.Add(value);
            }
            else
            {
                NotSelectedValues.Remove(value);
                SelectedValues.Add(value);
            }
        }
    }

    public class MultipleSelectorViewModel : ViewModelBase
    {
        private ObservableCollection<string> selectedValues;
        public ObservableCollection<string> SelectedValues
        {
            get => selectedValues;
            set
            {
                selectedValues = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> notSelectedValues;
        public ObservableCollection<string> NotSelectedValues
        {
            get => notSelectedValues;
            set
            {
                notSelectedValues = value;
                OnPropertyChanged();
            }
        }

        public MultipleSelectorViewModel()
        {
            SelectedValues = new ObservableCollection<string>();
            NotSelectedValues = new ObservableCollection<string>();
        }
    }
}
