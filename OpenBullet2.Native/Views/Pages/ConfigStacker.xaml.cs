using OpenBullet2.Core.Services;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Pages
{
    /// <summary>
    /// Interaction logic for ConfigStacker.xaml
    /// </summary>
    public partial class ConfigStacker : Page
    {
        private readonly ConfigService configService;
        private readonly ConfigStackerViewModel vm;

        public ConfigStacker()
        {
            configService = SP.GetService<ConfigService>();
            vm = SP.GetService<ViewModelsService>().ConfigStacker;
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

            vm.UpdateViewModel();
        }

        public void CreateBlock(BlockDescriptor descriptor) => vm.CreateBlock(descriptor);

        private void AddBlock(object sender, RoutedEventArgs e)
            => new MainDialog(new AddBlockDialog(this), "Add block").ShowDialog();

        private void RemoveBlock(object sender, RoutedEventArgs e) { }
        private void MoveBlockUp(object sender, RoutedEventArgs e) { }
        private void MoveBlockDown(object sender, RoutedEventArgs e) { }
        private void CloneBlock(object sender, RoutedEventArgs e) { }
        private void EnableDisableBlock(object sender, RoutedEventArgs e) { }
        private void Undo(object sender, RoutedEventArgs e) { }
    }
}
