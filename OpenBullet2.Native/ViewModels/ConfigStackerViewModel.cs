using OpenBullet2.Core.Services;
using RuriLib.Helpers;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigStackerViewModel : ViewModelBase
    {
        private readonly ConfigService configService;

        public event Action<IEnumerable<BlockViewModel>> SelectionChanged;

        private readonly List<(BlockInstance, int)> deletedBlocks = new();
        private BlockViewModel lastSelectedBlock = null;

        private ObservableCollection<BlockViewModel> stack;
        public ObservableCollection<BlockViewModel> Stack
        {
            get => stack;
            set
            {
                stack = value;
                OnPropertyChanged();
            }
        }

        public ConfigStackerViewModel()
        {
            configService = SP.GetService<ConfigService>();
            Stack = new();
        }

        public void CreateBlock(BlockDescriptor descriptor)
        {
            var newBlock = new BlockViewModel(BlockFactory.GetBlock<BlockInstance>(descriptor.Id));

            // If there are selected blocks, insert it after the last
            if (Stack.Any(b => b.Selected))
            {
                Stack.Insert(Stack.IndexOf(Stack.Last(b => b.Selected)) + 1, newBlock);
            }
            // Otherwise just add it at the end
            else
            {
                Stack.Add(newBlock);
            }

            SelectBlock(newBlock, false);
            SaveStack();
        }

        public void SelectBlock(BlockViewModel block, bool ctrl = false, bool shift = false)
        {
            // Clear the last selected block if it doesn't exist anymore
            if (!Stack.Contains(lastSelectedBlock))
            {
                lastSelectedBlock = null;
            }

            if (ctrl)
            {
                block.Selected = !block.Selected;
                lastSelectedBlock = block.Selected ? block : null;
            }
            else if (shift)
            {
                // If nothing was selected or already selected, simply select it
                if (lastSelectedBlock == null || lastSelectedBlock == block)
                {
                    block.Selected = true;
                    lastSelectedBlock = block;
                }
                // Otherwise get the last item of the list (which was selected last)
                // and select all the ones in between
                else
                {
                    foreach (var b in Stack)
                    {
                        b.Selected = false;
                    }

                    var lastSelectedBlockIndex = Stack.IndexOf(lastSelectedBlock);
                    var itemIndex = Stack.IndexOf(block);

                    var minIndex = Math.Min(lastSelectedBlockIndex, itemIndex);
                    var maxIndex = Math.Max(lastSelectedBlockIndex, itemIndex);

                    for (var i = minIndex; i <= maxIndex; i++)
                    {
                        Stack[i].Selected = true;
                    }

                    lastSelectedBlock = block;
                }
            }
            else
            {
                foreach (var b in Stack)
                {
                    b.Selected = false;
                }

                // If we called this from code passing null, simply clear the list. Otherwise:
                if (block != null)
                {
                    block.Selected = true;
                }

                lastSelectedBlock = block != null && block.Selected ? block : null;
            }

            SelectionChanged?.Invoke(Stack.Where(s => s.Selected));
        }

        public void RemoveSelected()
        {
            for (var i = 0; i < Stack.Count; i++)
            {
                var block = Stack[i];

                if (block.Selected)
                {
                    deletedBlocks.Add((block.Block, Stack.IndexOf(block)));
                    Stack.Remove(block);
                    i--;
                }
            }

            SelectBlock(null, false);
            SaveStack();
        }

        public void MoveSelectedUp()
        {
            // Start from the top and move the selected blocks up
            for (var i = 0; i < Stack.Count; i++)
            {
                var block = Stack[i];

                if (block.Selected && i > 0)
                {
                    Stack.RemoveAt(i);
                    Stack.Insert(i - 1, block);
                }
            }

            SaveStack();
        }

        public void MoveSelectedDown()
        {
            // Start from the bottom and move the selected blocks down
            for (var i = Stack.Count - 1; i >= 0; i--)
            {
                var block = Stack[i];

                if (block.Selected && i < Stack.Count - 1)
                {
                    Stack.RemoveAt(i);
                    Stack.Insert(i + 1, block);
                }
            }

            SaveStack();
        }

        public void CloneSelected()
        {
            var selected = Stack.Where(b => b.Selected).ToArray();

            foreach (var block in selected)
            {
                var newBlock = new BlockViewModel(Cloner.Clone(block.Block));

                Stack.Insert(Stack.IndexOf(block) + 1, newBlock);
            }

            SaveStack();
        }

        public void EnableDisableSelected()
        {
            foreach (var block in Stack.Where(b => b.Selected))
            {
                block.Disabled = !block.Disabled;
            }
        }

        public void Undo()
        {
            if (deletedBlocks.Count == 0)
            {
                return;
            }

            var toRestore = deletedBlocks.Last();
            deletedBlocks.Remove(toRestore);

            if (Stack.Count >= toRestore.Item2)
            {
                Stack.Insert(toRestore.Item2, new BlockViewModel(toRestore.Item1));
            }
            else
            {
                Stack.Add(new BlockViewModel(toRestore.Item1));
            }

            SaveStack();
        }

        public override void UpdateViewModel()
        {
            Stack = new ObservableCollection<BlockViewModel>(configService.SelectedConfig.Stack
                .Select(b => new BlockViewModel(b)));

            base.UpdateViewModel();
        }

        private void SaveStack() => configService.SelectedConfig.Stack = Stack.Select(b => b.Block).ToList();
    }

    public class BlockViewModel : ViewModelBase
    {
        public BlockInstance Block { get; init; }

        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        public string Label
        {
            get => Block.Label;
            set
            {
                Block.Label = value;
                OnPropertyChanged();
            }
        }

        public bool Disabled
        {
            get => Block.Disabled;
            set
            {
                Block.Disabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        public string BackgroundColor => Disabled ? "#444" : Block.Descriptor.Category.BackgroundColor;
        public string ForegroundColor => Disabled ? "#FFF" : Block.Descriptor.Category.ForegroundColor;

        public BlockViewModel(BlockInstance block)
        {
            Block = block;
        }
    }
}
