using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpenBullet2.Logging;
using OpenBullet2.Shared.Forms;
using RuriLib.Helpers;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class StackViewer
    {
        [Parameter] public List<BlockInstance> Stack { get; set; }
        [Parameter] public List<(BlockInstance, int)> DeletedBlocks { get; set; }
        [Parameter] public EventCallback<BlockInstance> SelectedBlock { get; set; }
        
        [Inject] private IModalService Modal { get; set; }
        [Inject] private BrowserConsoleLogger OBLogger { get; set; }
        
        private BlockInstance draggedItem;
        private List<BlockInstance> selectedBlocks = new();

        public void RefreshView()
        {
            StateHasChanged();
        }

        private void HandleDragStart(BlockInstance item)
        {
            if (item != null)
            {
                draggedItem = item;
            }
        }

        private void HandleDragOver(BlockInstance item)
        {
            if (item != null && draggedItem != null && !item.Equals(draggedItem))
            {
                Stack.Remove(draggedItem);
                Stack.Insert(Stack.IndexOf(item) + 1, draggedItem);
            }
        }

        private async Task SelectBlock(BlockInstance item, MouseEventArgs e)
        {
            // If we click while holding down CTRL
            if (e != null && e.CtrlKey)
            {
                // If it was already selected, remove it
                if (selectedBlocks.Contains(item))
                {
                    selectedBlocks.Remove(item);
                }
                // Otherwise, add it
                else
                {
                    selectedBlocks.Add(item);
                    await SelectedBlock.InvokeAsync(item);
                }
            }
            // If we clicked while holding SHIFT
            else if (e != null && e.ShiftKey)
            {
                // If nothing was selected, simply select it
                if (selectedBlocks.Count == 0)
                {
                    selectedBlocks.Add(item);
                }
                // Otherwise get the last item of the list (which was selected last)
                // and select all the ones in between
                else
                {
                    var lastSelectedBlock = selectedBlocks.Last();
                    selectedBlocks.Clear();

                    var lastSelectedBlockIndex = Stack.IndexOf(lastSelectedBlock);
                    var itemIndex = Stack.IndexOf(item);

                    var minIndex = Math.Min(lastSelectedBlockIndex, itemIndex);
                    var maxIndex = Math.Max(lastSelectedBlockIndex, itemIndex);

                    for (var i = minIndex; i <= maxIndex; i++)
                    {
                        selectedBlocks.Add(Stack[i]);
                    }
                }

                await SelectedBlock.InvokeAsync(item);
            }
            // Otherwise if we simply clicked on a block
            else
            {
                selectedBlocks.Clear();

                // If we called this from code passing null, simply clear the list
                if (item != null)
                {
                    selectedBlocks.Add(item);
                }
                
                await SelectedBlock.InvokeAsync(item);
            }
        }

        private async Task AddBlock()
        {
            var modal = Modal.Show<AddBlock>(Loc["AddBlock"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var descriptor = (BlockDescriptor)result.Data;
                var newBlock = BlockFactory.GetBlock<BlockInstance>(descriptor.Id);

                // If there are selected blocks, insert it after the last
                if (selectedBlocks.Any())
                {
                    Stack.Insert(Stack.IndexOf(selectedBlocks.Last()) + 1, newBlock);
                }
                // Otherwise just add it at the end
                else
                {
                    Stack.Add(newBlock);
                }

                await OBLogger.LogInfo($"Added block {newBlock.Id}");
                await SelectBlock(newBlock, null);
            }
        }

        private async Task DeleteSelectedBlocks()
        {
            foreach (var block in selectedBlocks)
            {
                DeletedBlocks.Add((block, Stack.IndexOf(block)));
                Stack.Remove(block);
                await OBLogger.LogInfo($"Deleted block {block.Id}");
            }
            
            await SelectBlock(null, null);
        }

        private async Task Undo()
        {
            if (DeletedBlocks.Count == 0)
                return;

            var toRestore = DeletedBlocks.Last();
            DeletedBlocks.Remove(toRestore);

            if (Stack.Count >= toRestore.Item2)
            {
                Stack.Insert(toRestore.Item2, toRestore.Item1);
            }
            else
            {
                Stack.Add(toRestore.Item1);
            }

            await OBLogger.LogInfo($"Restored block {toRestore.Item1.Id}");
        }

        private async Task DisableBlocks()
        {
            foreach (var block in selectedBlocks.Where(b => b is not LoliCodeBlockInstance))
            {
                block.Disabled = !block.Disabled;
                await OBLogger.LogInfo($"Toggled disabled for block {block.Id}");
            }
        }

        private async Task MoveBlocksUp()
        {
            // Start from the top and move the selected blocks up
            for (var i = 0; i < Stack.Count; i++)
            {
                var block = Stack[i];

                if (selectedBlocks.Contains(block) && i > 0)
                {
                    Stack.RemoveAt(i);
                    Stack.Insert(i - 1, block);
                    await OBLogger.LogInfo($"Moved up block {block.Id}");
                }
            }
        }

        private async Task MoveBlocksDown()
        {
            // Start from the bottom and move the selected blocks down
            for (var i = Stack.Count - 1; i >= 0; i--)
            {
                var block = Stack[i];

                if (selectedBlocks.Contains(block) && i < Stack.Count - 1)
                {
                    Stack.RemoveAt(i);
                    Stack.Insert(i + 1, block);
                    await OBLogger.LogInfo($"Moved down block {block.Id}");
                }
            }
        }

        private async Task CloneBlocks()
        {
            foreach (var block in selectedBlocks)
            {
                var newBlock = Cloner.Clone(block);

                Stack.Insert(Stack.IndexOf(block) + 1, newBlock);
                await OBLogger.LogInfo($"Cloned block {block.Id}");
            }
        }
    }
}
