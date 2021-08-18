using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for KeychainViewer.xaml
    /// </summary>
    public partial class KeychainViewer : UserControl
    {
        private readonly KeychainViewerViewModel vm;

        public Keychain Keychain { get; init; }
        public event EventHandler OnDeleted;
        public event EventHandler OnMoveUp;
        public event EventHandler OnMoveDown;

        public KeychainViewer(Keychain keychain)
        {
            vm = new KeychainViewerViewModel(keychain);
            DataContext = vm;
            Keychain = keychain;

            InitializeComponent();
            keychain.Keys.ForEach(k => DisplayKey(k));
        }

        private void Delete(object sender, RoutedEventArgs e) => OnDeleted?.Invoke(this, EventArgs.Empty);
        private void MoveUp(object sender, RoutedEventArgs e) => OnMoveUp?.Invoke(this, EventArgs.Empty);
        private void MoveDown(object sender, RoutedEventArgs e) => OnMoveDown?.Invoke(this, EventArgs.Empty);

        private void AddStringKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateStringKey());
        private void AddBoolKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateBoolKey());
        private void AddIntKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateIntKey());
        private void AddFloatKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateFloatKey());
        private void AddListKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateListKey());
        private void AddDictionaryKey(object sender, RoutedEventArgs e) => DisplayKey(vm.CreateDictionaryKey());

        private void DisplayKey(Key key)
        {
            var view = new KeyViewer(key);
            view.OnDeleted += (s, e) =>
            {
                vm.DeleteKey(key);
                keysPanel.Children.Remove(view);
            };
            keysPanel.Children.Add(view);
        }
    }

    public class KeychainViewerViewModel : ViewModelBase
    {
        private readonly RuriLibSettingsService rlSettingsService;
        private readonly Keychain keychain;

        public KeychainViewerViewModel(Keychain keychain)
        {
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
            this.keychain = keychain;
        }

        public SolidColorBrush BorderBrush => ResultStatus switch
        {
            "SUCCESS" => Brushes.YellowGreen,
            "FAIL" => Brushes.Tomato,
            "RETRY" => Brushes.Yellow,
            "BAN" => Brushes.Plum,
            "NONE" => Brushes.SkyBlue,
            _ => Brushes.Orange
        };

        public string ResultStatus
        {
            get => keychain.ResultStatus;
            set
            {
                keychain.ResultStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BorderBrush));
            }
        }

        public IEnumerable<string> Statuses => rlSettingsService.GetStatuses();

        public KeychainMode Mode
        {
            get => keychain.Mode;
            set
            {
                keychain.Mode = value;
                OnPropertyChanged();
            }
        }

        public StringKey CreateStringKey()
        {
            var key = new StringKey();
            keychain.Keys.Add(key);
            return key;
        }

        public BoolKey CreateBoolKey()
        {
            var key = new BoolKey();
            keychain.Keys.Add(key);
            return key;
        }

        public IntKey CreateIntKey()
        {
            var key = new IntKey();
            keychain.Keys.Add(key);
            return key;
        }

        public FloatKey CreateFloatKey()
        {
            var key = new FloatKey();
            keychain.Keys.Add(key);
            return key;
        }

        public ListKey CreateListKey()
        {
            var key = new ListKey();
            keychain.Keys.Add(key);
            return key;
        }

        public DictionaryKey CreateDictionaryKey()
        {
            var key = new DictionaryKey();
            keychain.Keys.Add(key);
            return key;
        }

        public void DeleteKey(Key key) => keychain.Keys.Remove(key);

        public IEnumerable<KeychainMode> Modes => Enum.GetValues(typeof(KeychainMode)).Cast<KeychainMode>();
    }
}
