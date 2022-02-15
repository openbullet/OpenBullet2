using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Keycheck;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for KeycheckBlockSettingsViewer.xaml
    /// </summary>
    public partial class KeycheckBlockSettingsViewer : UserControl
    {
        private readonly KeycheckBlockSettingsViewerViewModel vm;

        public KeycheckBlockSettingsViewer(BlockViewModel blockVM)
        {
            if (blockVM.Block is not KeycheckBlockInstance)
            {
                throw new Exception("Wrong block type for this UC");
            }

            vm = new KeycheckBlockSettingsViewerViewModel(blockVM);
            DataContext = vm;

            InitializeComponent();
            BindSettings();
        }

        // TODO: Find a way to automatically scout the visual tree and get the settings viewers by Tag
        // to set their Setting property automatically basing on the Tag instead of doing it manually
        private void BindSettings()
        {
            banIfNoMatchSetting.Setting = vm.KeycheckBlock.Settings["banIfNoMatch"];
            vm.KeycheckBlock.Keychains.ForEach(k => SpawnKeychain(k));
        }

        private void AddKeychain(object sender, RoutedEventArgs e)
        {
            var keychain = vm.CreateKeychain();
            SpawnKeychain(keychain);
        }

        private void SpawnKeychain(Keychain keychain)
        {
            var view = new KeychainViewer(keychain);

            view.OnDeleted += (s, e) =>
            {
                vm.DeleteKeychain(view.Keychain);
                keychainsPanel.Children.Remove(view);
            };
            
            view.OnMoveUp += (s, e) =>
            {
                var index = keychainsPanel.Children.IndexOf(view);

                if (index > 0)
                {
                    keychainsPanel.Children.RemoveAt(index);
                    keychainsPanel.Children.Insert(index - 1, view);
                }

                vm.MoveKeychainUp(keychain);
            };
            
            view.OnMoveDown += (s, e) =>
            {
                var index = keychainsPanel.Children.IndexOf(view);

                if (index < keychainsPanel.Children.Count - 1)
                {
                    keychainsPanel.Children.RemoveAt(index);
                    keychainsPanel.Children.Insert(index + 1, view);
                }

                vm.MoveKeychainDown(keychain);
            };

            keychainsPanel.Children.Add(view);
        }
    }

    public class KeycheckBlockSettingsViewerViewModel : BlockSettingsViewerViewModel
    {
        public KeycheckBlockInstance KeycheckBlock => Block as KeycheckBlockInstance;

        public KeycheckBlockSettingsViewerViewModel(BlockViewModel block) : base(block)
        {

        }

        public Keychain CreateKeychain()
        {
            var keychain = new Keychain();
            KeycheckBlock.Keychains.Add(keychain);
            return keychain;
        }

        public void DeleteKeychain(Keychain keychain) => KeycheckBlock.Keychains.Remove(keychain);

        public void MoveKeychainUp(Keychain keychain)
        {
            var index = KeycheckBlock.Keychains.IndexOf(keychain);

            if (index > 0)
            {
                KeycheckBlock.Keychains.RemoveAt(index);
                KeycheckBlock.Keychains.Insert(index - 1, keychain);
            }
        }

        public void MoveKeychainDown(Keychain keychain)
        {
            var index = KeycheckBlock.Keychains.IndexOf(keychain);

            if (index < KeycheckBlock.Keychains.Count - 1)
            {
                KeycheckBlock.Keychains.RemoveAt(index);
                KeycheckBlock.Keychains.Insert(index + 1, keychain);
            }
        }
    }
}
