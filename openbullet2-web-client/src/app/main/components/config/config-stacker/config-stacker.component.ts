import { Component, OnInit, ViewChild } from '@angular/core';
import { faArrowDown, faArrowUp, faBan, faClone, faGripLines, faPlus, faRotateLeft, faSearch, faTrashCan, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptorDto, BlockDescriptors } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockInstanceTypes } from 'src/app/main/dtos/config/block-instance.dto';
import { CategoryTreeNode } from 'src/app/main/dtos/config/category-tree.dto';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { AddBlockComponent } from './add-block/add-block.component';

interface DeletedBlock {
  block: BlockInstanceTypes;
  index: number;
}

@Component({
  selector: 'app-config-stacker',
  templateUrl: './config-stacker.component.html',
  styleUrls: ['./config-stacker.component.scss']
})
export class ConfigStackerComponent implements OnInit {
  config: ConfigDto | null = null;
  stack: BlockInstanceTypes[] | null = null;
  descriptors: BlockDescriptors | null = null;
  categoryTree: CategoryTreeNode | null = null;
  selectedBlocks: BlockInstanceTypes[] = [];
  lastSelectedBlock: BlockInstanceTypes | null = null;
  lastDeletedBlocks: DeletedBlock[] = [];

  faPlus = faPlus;
  faTrashCan = faTrashCan;
  faArrowUp = faArrowUp;
  faArrowDown = faArrowDown;
  faBan = faBan;
  faClone = faClone;
  faRotateLeft = faRotateLeft;

  addBlockModalVisible = false;

  @ViewChild('addBlockComponent')
  addBlockComponent: AddBlockComponent | undefined = undefined;

  // This is not present in the descriptors returned from the API
  // but we need it to display the LoliCode script block
  loliCodeBlockDescriptor: BlockDescriptorDto = {
    id: 'loliCode',
    name: 'LoliCode',
    description: 'This block can hold a LoliCode script',
    extraInfo: '',
    returnType: null,
    category: {
      description: 'Category for the LoliCode script block',
      backgroundColor: '#303030',
      foregroundColor: '#fff',
      name: 'LoliCode',
    },
    parameters: {}
  }

  faGripLines = faGripLines;
  faTriangleExclamation = faTriangleExclamation;

  constructor(private configService: ConfigService) {
    this.configService.selectedConfig$
      .subscribe(config => this.config = config);
  }

  ngOnInit(): void {
    if (this.config === null) {
      return;
    }

    this.configService.convertLoliCodeToStack(this.config.loliCodeScript)
      .subscribe(resp => {
        this.stack = resp.stack;
      });

    this.configService.getBlockDescriptors()
      .subscribe(descriptors => {
        this.descriptors = descriptors;

        this.configService.getCategoryTree()
          .subscribe(tree => this.categoryTree = new CategoryTreeNode(tree, null, descriptors));
      });
  }

  getDescriptor(block: BlockInstanceTypes): BlockDescriptorDto {
    if (block.id === 'loliCode') {
      return this.loliCodeBlockDescriptor;
    }

    if (this.descriptors === null) {
      throw new Error('Descriptors not loaded yet');
    }

    const descriptor = this.descriptors[block.id];

    if (descriptor === undefined) {
      throw new Error(`Could not find descriptor for block ${block.id}`);
    }

    return descriptor;
  }

  getBackgroundColor(block: BlockInstanceTypes): string {
    const descriptor = this.getDescriptor(block);
    return descriptor.category?.backgroundColor ?? 'black';
  }

  getForegroundColor(block: BlockInstanceTypes): string {
    const descriptor = this.getDescriptor(block);
    return descriptor.category?.foregroundColor ?? 'white';
  }

  isSelected(block: BlockInstanceTypes): boolean {
    return this.selectedBlocks.includes(block);
  }

  selectBlock(block: BlockInstanceTypes, event: MouseEvent) {
    // If the user is holding shift, select all blocks between the last selected block and this one
    if (event.shiftKey && this.lastSelectedBlock !== null) {
      const startIndex = this.stack?.indexOf(this.lastSelectedBlock) ?? -1;
      const endIndex = this.stack?.indexOf(block) ?? -1;

      if (startIndex === -1 || endIndex === -1) {
        return;
      }

      const start = Math.min(startIndex, endIndex);
      const end = Math.max(startIndex, endIndex);

      this.selectedBlocks = this.stack?.slice(start, end + 1) ?? [];
      this.lastSelectedBlock = block;
      return;
    }

    // If the user is holding ctrl, toggle the selection of this block
    if (event.ctrlKey) {
      if (this.selectedBlocks.includes(block)) {
        this.selectedBlocks = this.selectedBlocks.filter(b => b !== block);
      } else {
        this.selectedBlocks.push(block);
      }

      this.lastSelectedBlock = block;
      return;
    }

    // Otherwise, just select this block
    this.selectedBlocks = [block];
    this.lastSelectedBlock = block;
  }

  openAddBlockModal() {
    this.addBlockComponent?.resetCategory();
    this.addBlockModalVisible = true;
  }

  addBlock(block: BlockInstanceTypes) {
    if (this.stack === null) {
      return;
    }

    // Add the block to the end of the stack
    this.stack.push(block);

    // Select the block
    this.selectedBlocks = [block];
    this.lastSelectedBlock = block;

    // Close the modal
    this.addBlockModalVisible = false;
  }

  deleteBlocks() {
    if (this.stack === null) {
      return;
    }

    // Add the selected blocks to the deleted blocks list
    this.lastDeletedBlocks = [];
    for (const block of this.selectedBlocks) {
      const index = this.stack.indexOf(block);
      this.lastDeletedBlocks.push({ block, index });
    }

    // Remove the selected blocks from the stack
    this.stack = this.stack.filter(block => !this.selectedBlocks.includes(block));

    // Clear the selection
    this.selectedBlocks = [];
    this.lastSelectedBlock = null;
  }

  moveBlocksUp() {
    if (this.stack === null) {
      return;
    }

    // Sort the selected blocks by their index in the stack
    this.selectedBlocks.sort((a, b) => {
      const aIndex = this.stack?.indexOf(a) ?? -1;
      const bIndex = this.stack?.indexOf(b) ?? -1;
      return aIndex - bIndex;
    });

    // Move the selected blocks up, starting from the top
    for (let i = 0; i < this.selectedBlocks.length; i++) {
      const block = this.selectedBlocks[i];
      const index = this.stack.indexOf(block);

      if (index === 0) {
        continue;
      }

      // If the block above is also selected, skip this block
      if (this.selectedBlocks.includes(this.stack[index - 1])) {
        continue;
      }

      this.stack.splice(index, 1);
      this.stack.splice(index - 1, 0, block);
    }
  }

  moveBlocksDown() {
    if (this.stack === null) {
      return;
    }

    // Sort the selected blocks by their index in the stack
    this.selectedBlocks.sort((a, b) => {
      const aIndex = this.stack?.indexOf(a) ?? -1;
      const bIndex = this.stack?.indexOf(b) ?? -1;
      return aIndex - bIndex;
    });

    // Move the selected blocks down, starting from the bottom
    for (let i = this.selectedBlocks.length - 1; i >= 0; i--) {
      const block = this.selectedBlocks[i];
      const index = this.stack.indexOf(block);

      if (index === this.stack.length - 1) {
        continue;
      }

      // If the block below is also selected, skip this block
      if (this.selectedBlocks.includes(this.stack[index + 1])) {
        continue;
      }

      this.stack.splice(index, 1);
      this.stack.splice(index + 1, 0, block);
    }
  }

  cloneBlocks() {
    if (this.stack === null) {
      return;
    }

    // Clone the selected blocks
    for (const block of this.selectedBlocks) {
      const index = this.stack.indexOf(block);
      const clone = JSON.parse(JSON.stringify(block));
      this.stack.splice(index + 1, 0, clone);
    }
  }

  toggleDisabled() {
    // Toggle the disabled state of the selected blocks
    for (const block of this.selectedBlocks) {
      block.disabled = !block.disabled;
    }
  }

  restoreDeletedBlocks() {
    if (this.stack === null) {
      return;
    }

    // Restore the deleted blocks
    for (const deletedBlock of this.lastDeletedBlocks) {
      // If the index is out of bounds, just push the block to the end of the stack
      if (deletedBlock.index >= this.stack.length) {
        this.stack.push(deletedBlock.block);
        continue;
      }

      this.stack.splice(deletedBlock.index, 0, deletedBlock.block);
    }

    // Clear the deleted blocks list
    this.lastDeletedBlocks = [];
  }
}
