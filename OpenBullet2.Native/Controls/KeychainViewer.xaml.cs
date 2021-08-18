using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Services;
using System;
using System.Collections;
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
        }

        private void Delete(object sender, RoutedEventArgs e) => OnDeleted?.Invoke(this, EventArgs.Empty);
        private void MoveUp(object sender, RoutedEventArgs e) => OnMoveUp?.Invoke(this, EventArgs.Empty);
        private void MoveDown(object sender, RoutedEventArgs e) => OnMoveDown?.Invoke(this, EventArgs.Empty);
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

        public IEnumerable<KeychainMode> Modes => Enum.GetValues(typeof(KeychainMode)).Cast<KeychainMode>();
    }
}
