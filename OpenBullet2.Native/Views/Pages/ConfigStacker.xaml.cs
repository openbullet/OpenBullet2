using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Controls;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigStacker.xaml
    /// </summary>
    public partial class ConfigStacker : Page
    {
        private readonly ConfigService configService;
        private readonly IConfigRepository configRepo;
        private readonly ConfigStackerViewModel vm;

        public ConfigStacker()
        {
            configService = SP.GetService<ConfigService>();
            configRepo = SP.GetService<IConfigRepository>();
            vm = SP.GetService<ViewModelsService>().ConfigStacker;
            vm.SelectionChanged += SelectionChanged;
            DataContext = vm;

            InitializeComponent();
        }

        public void UpdateViewModel()
        {
            try
            {
                // Try to change the mode to Stack
                configService.SelectedConfig.ChangeMode(ConfigMode.Stack);
            }
            catch (Exception ex)
            {
                // On fail, prompt it to the user and go back to the configs page
                Alert.Exception(ex);
                SP.GetService<MainWindow>().NavigateTo(MainWindowPage.Configs);
            }

            vm.SelectBlock(null, false);
            vm.UpdateViewModel();
        }

        public void CreateBlock(BlockDescriptor descriptor) => vm.CreateBlock(descriptor);

        private void AddBlock(object sender, RoutedEventArgs e)
            => new MainDialog(new AddBlockDialog(this), "Add block").ShowDialog();

        private void RemoveBlock(object sender, RoutedEventArgs e) => vm.RemoveSelected();
        private void MoveBlockUp(object sender, RoutedEventArgs e) => vm.MoveSelectedUp();
        private void MoveBlockDown(object sender, RoutedEventArgs e) => vm.MoveSelectedDown();
        private void CloneBlock(object sender, RoutedEventArgs e) => vm.CloneSelected();
        private void EnableDisableBlock(object sender, RoutedEventArgs e) => vm.EnableDisableSelected();
        private void Undo(object sender, RoutedEventArgs e) => vm.Undo();

        private void SelectBlock(object sender, MouseEventArgs e) => SelectBlock(sender);
        private void SelectBlock(object sender, RoutedEventArgs e) => SelectBlock(sender);
        private void SelectBlock(object sender)
        {
            var ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            var shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            var block = (BlockViewModel)(sender as FrameworkElement).Tag;
            vm.SelectBlock(block, ctrl, shift);
        }

        private void SelectionChanged(IEnumerable<BlockViewModel> selected)
        {
            var first = selected.FirstOrDefault();

            if (first is null)
            {
                blockInfo.Content = null;
            }
            else
            {
                UserControl content = first.Block switch
                {
                    AutoBlockInstance => new AutoBlockSettingsViewer(first),
                    ParseBlockInstance => new ParseBlockSettingsViewer(first),
                    ScriptBlockInstance => new ScriptBlockSettingsViewer(first),
                    HttpRequestBlockInstance => new HttpRequestBlockSettingsViewer(first),
                    KeycheckBlockInstance => new KeycheckBlockSettingsViewer(first),
                    LoliCodeBlockInstance => new LoliCodeBlockSettingsViewer(first),
                    _ => null
                };

                blockInfo.Content = content;
            }
        }

        private async void PageKeyDown(object sender, KeyEventArgs e)
        {
            // Save on CTRL+S
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                await configRepo.Save(configService.SelectedConfig);
                Alert.Success("Saved", $"{configService.SelectedConfig.Metadata.Name} was saved successfully!");
            }
        }
    }
}
