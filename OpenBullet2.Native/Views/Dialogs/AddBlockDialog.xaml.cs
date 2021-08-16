using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Pages;
using RuriLib.Models.Blocks;
using RuriLib.Models.Trees;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            filterTextBox.Focus();
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

        private void SelectCategory(object sender, RoutedEventArgs e)
            => vm.SelectCategory((CategoryTreeNode)(sender as Button).Tag);

        private void SelectDescriptor(object sender, RoutedEventArgs e) => SelectDescriptor(sender);
        private void SelectDescriptor(object sender, MouseEventArgs e) => SelectDescriptor(sender);
        private void SelectDescriptor(object sender)
        {
            var descriptor = (BlockDescriptor)(sender as FrameworkElement).Tag;
            vm.SelectDescriptor(descriptor);

            if (caller is ConfigStacker page)
            {
                page.CreateBlock(descriptor);
            }

            ((MainDialog)Parent).Close();
        }

        private void Search(object sender, RoutedEventArgs e) => vm.Filter = filterTextBox.Text;
        private void FilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.Filter = filterTextBox.Text;
            }
        }
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

                CreateCollection();
            }
        }

        public bool CanGoUp => CurrentNode.Parent is not null;

        private CompositeCollection nodesCollection;
        public CompositeCollection NodesCollection
        {
            get => nodesCollection;
            set
            {
                nodesCollection = value;
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
                OnPropertyChanged();
                CreateCollection();
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

            RecentDescriptors = new ObservableCollection<BlockDescriptor>(volatileSettings.RecentDescriptors.Take(8));
        }

        public void SelectDescriptor(BlockDescriptor descriptor) => volatileSettings.AddRecentDescriptor(descriptor);

        public void SelectCategory(CategoryTreeNode node)
        {
            CurrentNode = node;
            CreateCollection();
        }

        public void GoUp()
        {
            if (CurrentNode.Parent is not null)
            {
                CurrentNode = CurrentNode.Parent;
            }
        }

        private void CreateCollection()
        {
            ObservableCollection<CategoryTreeNode> subCategories;
            ObservableCollection<BlockDescriptor> descriptors;

            if (string.IsNullOrWhiteSpace(filter))
            {
                subCategories = new ObservableCollection<CategoryTreeNode>(currentNode.SubCategories);
                descriptors = new ObservableCollection<BlockDescriptor>(currentNode.Descriptors);
            }
            else
            {
                subCategories = new();
                descriptors = new ObservableCollection<BlockDescriptor>(RuriLib.Globals.DescriptorsRepository.Descriptors.Values
                    .Where(d => d.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)));
            }
            
            var composite = new CompositeCollection
            {
                new CollectionContainer { Collection = subCategories },
                new CollectionContainer { Collection = descriptors }
            };

            NodesCollection = composite;
        }
    }
}
