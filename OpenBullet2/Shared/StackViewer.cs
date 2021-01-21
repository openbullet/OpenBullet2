using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Logging;
using OpenBullet2.Shared.Forms;
using RuriLib.Helpers;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
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
        [Inject] public IModalService Modal { get; set; }
        [Inject] public BrowserConsoleLogger OBLogger { get; set; }
        private BlockInstance draggedItem;
        private BlockInstance selectedBlock;

        public void RefreshView()
        {
            StateHasChanged();
        }

        private void HandleDragStart(BlockInstance item)
        {
            draggedItem = item;
        }

        private void HandleDragOver(BlockInstance item)
        {
            if (!item.Equals(draggedItem))
            {
                Stack.Remove(draggedItem);
                Stack.Insert(Stack.IndexOf(item) + 1, draggedItem);
            }
        }

        private async Task SelectBlock(BlockInstance item)
        {
            selectedBlock = item;
            await SelectedBlock.InvokeAsync(item);

            if (selectedBlock == null)
                await OBLogger.LogInfo("Deselected blocks");
            else
                await OBLogger.LogInfo($"Selected block {item.Id}");
        }

        private async Task AddBlock()
        {
            var modal = Modal.Show<AddBlock>(Loc["AddBlock"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var descriptor = (BlockDescriptor)result.Data;
                var newBlock = BlockFactory.GetBlock<BlockInstance>(descriptor.Id);

                if (selectedBlock != null)
                {
                    Stack.Insert(Stack.IndexOf(selectedBlock) + 1, newBlock);
                }
                else
                {
                    Stack.Add(newBlock);
                }

                selectedBlock = newBlock;
                await OBLogger.LogInfo($"Added block {selectedBlock.Id}");
            }
        }

        private async Task DeleteSelectedBlock()
        {
            if (selectedBlock == null)
                return;

            DeletedBlocks.Add((selectedBlock, Stack.IndexOf(selectedBlock)));
            Stack.Remove(selectedBlock);
            await OBLogger.LogInfo($"Deleted block {selectedBlock.Id}");
            await SelectBlock(null);
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

        private async Task DisableBlock()
        {
            if (selectedBlock == null)
                return;

            selectedBlock.Disabled = !selectedBlock.Disabled;
            await OBLogger.LogInfo($"Toggled disabled for block {selectedBlock.Id}");
        }

        private async Task MoveBlockUp()
        {
            if (selectedBlock == null)
                return;

            var index = Stack.IndexOf(selectedBlock);

            if (index == 0)
                return;

            Stack.Remove(selectedBlock);
            Stack.Insert(index - 1, selectedBlock);
            await OBLogger.LogInfo($"Moved up block {selectedBlock.Id}");
        }

        private async Task MoveBlockDown()
        {
            if (selectedBlock == null)
                return;

            var index = Stack.IndexOf(selectedBlock);

            if (index == Stack.Count - 1)
                return;

            Stack.Remove(selectedBlock);
            Stack.Insert(index + 1, selectedBlock);
            await OBLogger.LogInfo($"Moved down block {selectedBlock.Id}");
        }

        private async Task CloneBlock()
        {
            if (selectedBlock == null)
                return;

            var newBlock = Cloner.Clone(selectedBlock);

            Stack.Insert(Stack.IndexOf(selectedBlock) + 1, newBlock);
            await OBLogger.LogInfo($"Cloned block {selectedBlock.Id}");
        }
    }
}
