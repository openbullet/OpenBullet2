using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            new PropertyMetadata(default(ObservableCollection<string>), OnSelectedValuesPropertyChanged));

        private static void OnSelectedValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as MultipleSelector;
            source.UpdateControls();
        }

        public IEnumerable<string> PossibleValues
        {
            get => (IEnumerable<string>)GetValue(PossibleValuesProperty);
            set => SetValue(PossibleValuesProperty, value);
        }

        public static readonly DependencyProperty PossibleValuesProperty =
        DependencyProperty.Register(
            nameof(PossibleValues),
            typeof(IEnumerable<string>),
            typeof(MultipleSelector),
            new PropertyMetadata(default(IEnumerable<string>), OnPossibleValuesPropertyChanged));

        private static void OnPossibleValuesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as MultipleSelector;
            source.UpdateControls();
        }

        public void UpdateControls()
        {
            if (SelectedValues is not null)
            {
                selectedValuesControl.Items.Clear();

                foreach (var value in SelectedValues)
                {
                    selectedValuesControl.Items.Add(value);
                }
            }

            if (PossibleValues is not null && SelectedValues is not null)
            {
                notSelectedValuesControl.Items.Clear();

                foreach (var value in PossibleValues.Where(v => !SelectedValues.Contains(v)))
                {
                    notSelectedValuesControl.Items.Add(value);
                }
            }
        }

        public MultipleSelector()
        {
            InitializeComponent();
        }

        private void MoveAllRight(object sender, RoutedEventArgs e)
        {
            SelectedValues.Clear();
            selectedValuesControl.Items.Clear();
            notSelectedValuesControl.Items.Clear();

            foreach (var value in PossibleValues)
            {
                notSelectedValuesControl.Items.Add(value);
            }

            SelectedValues = SelectedValues; // HACK: Needed to notify...
        }

        private void MoveAllLeft(object sender, RoutedEventArgs e)
        {
            SelectedValues.Clear();
            selectedValuesControl.Items.Clear();
            notSelectedValuesControl.Items.Clear();

            foreach (var value in PossibleValues)
            {
                selectedValuesControl.Items.Add(value);
            }

            SelectedValues = SelectedValues;
        }

        private void MoveItem(object sender, RoutedEventArgs e)
        {
            var value = (sender as Label).Content as string;
            
            if (SelectedValues.Contains(value))
            {
                SelectedValues.Remove(value);
                selectedValuesControl.Items.Remove(value);
                notSelectedValuesControl.Items.Add(value);
            }
            else
            {
                SelectedValues.Add(value);
                selectedValuesControl.Items.Add(value);
                notSelectedValuesControl.Items.Remove(value);
            }

            SelectedValues = SelectedValues;
        }
    }
}
