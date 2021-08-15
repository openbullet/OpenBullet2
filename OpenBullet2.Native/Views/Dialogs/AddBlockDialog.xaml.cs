using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks;
using RuriLib.Models.Trees;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddBlockDialog.xaml
    /// </summary>
    public partial class AddBlockDialog : Page
    {
        private readonly AddBlockDialogViewModel vm;
        private readonly object caller;

        public AddBlockDialog(object caller)
        {
            this.caller = caller;

            vm = new AddBlockDialogViewModel();
            DataContext = vm;

            InitializeComponent();
        }

        private void GoUp(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(vm.Filter))
            {
                filterTextBox.Text = string.Empty;
                vm.Filter = string.Empty;
            }
            else
            {
                vm.GoUp();
            }
        }

        private void Search(object sender, RoutedEventArgs e) => vm.Filter = filterTextBox.Text;
        private void FilterKeyDown(object sender, KeyEventArgs e) => vm.Filter = filterTextBox.Text;
    }

    public class AddBlockDialogViewModel : ViewModelBase
    {
        private readonly VolatileSettingsService volatileSettings;

        private CategoryTreeNode currentNode;
        public CategoryTreeNode CurrentNode
        {
            get => currentNode;
            set
            {
                currentNode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoUp));
            }
        }

        public bool CanGoUp() => CurrentNode.Parent is not null;

        private ObservableCollection<CategoryTreeNode> subCategories;
        public ObservableCollection<CategoryTreeNode> SubCategories
        {
            get => subCategories;
            set
            {
                subCategories = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BlockDescriptor> descriptors;
        public ObservableCollection<BlockDescriptor> Descriptors
        {
            get => descriptors;
            set
            {
                descriptors = value;
                OnPropertyChanged();
            }
        }

        private string filter = string.Empty;
        public string Filter
        {
            get => filter;
            set
            {
                filter = value;
                Descriptors = new ObservableCollection<BlockDescriptor>(RuriLib.Globals.DescriptorsRepository.Descriptors.Values
                    .Where(d => d.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)));
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BlockDescriptor> recentDescriptors;
        public ObservableCollection<BlockDescriptor> RecentDescriptors
        {
            get => recentDescriptors;
            set
            {
                recentDescriptors = value;
                OnPropertyChanged();
            }
        }

        public AddBlockDialogViewModel()
        {
            volatileSettings = SP.GetService<VolatileSettingsService>();

            var root = RuriLib.Globals.DescriptorsRepository.AsTree();
            CurrentNode = root
                .SubCategories.First(s => s.Name == "RuriLib")
                .SubCategories.First(s => s.Name == "Blocks");

            RecentDescriptors = new ObservableCollection<BlockDescriptor>(volatileSettings.RecentDescriptors.Take(6));
        }

        public void SelectDescriptor(BlockDescriptor descriptor) => volatileSettings.AddRecentDescriptor(descriptor);

        public void SelectCategory(CategoryTreeNode node)
        {
            CurrentNode = node;
            SubCategories = new ObservableCollection<CategoryTreeNode>(currentNode.SubCategories);
        }

        public void GoUp()
        {
            if (CurrentNode.Parent is not null)
            {
                CurrentNode = CurrentNode.Parent;
            }
        }
    }
}
