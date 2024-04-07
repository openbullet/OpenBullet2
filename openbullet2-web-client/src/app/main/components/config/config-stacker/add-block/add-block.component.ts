import { Component, EventEmitter, Input, Output } from '@angular/core';
import { faArrowUp, faSearch } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptorDto, BlockDescriptors } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockInstanceTypes } from 'src/app/main/dtos/config/block-instance.dto';
import { CategoryTreeNode } from 'src/app/main/dtos/config/category-tree.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { VolatileSettingsService } from 'src/app/main/services/volatile-settings.service';

@Component({
  selector: 'app-add-block',
  templateUrl: './add-block.component.html',
  styleUrls: ['./add-block.component.scss'],
})
export class AddBlockComponent {
  @Input() tree: CategoryTreeNode | null = null;
  @Input() descriptorsRepo: BlockDescriptors | null = null;
  @Output() blockSelected = new EventEmitter<BlockInstanceTypes>();
  currentCategory: CategoryTreeNode | null = null;
  recentlyUsedBlockIds: string[] = [];
  searchFilter = '';
  subCategories: CategoryTreeNode[] = [];
  descriptors: BlockDescriptorDto[] = [];

  faArrowUp = faArrowUp;
  faSearch = faSearch;

  // Prevents the tooltip from staying open when the modal is closed
  tooltipDisabled = false;

  constructor(
    private configService: ConfigService,
    private volatileSettingsService: VolatileSettingsService,
  ) {
    this.recentlyUsedBlockIds = volatileSettingsService.recentlyUsedBlockIds;
  }

  public resetCategory() {
    if (this.tree !== null) {
      const mainNode = this.tree.subCategories
        .find((c) => c.name === 'RuriLib')!
        .subCategories.find((c) => c.name === 'Blocks')!;

      this.searchFilter = '';
      this.setcurrentCategory(mainNode);
    }

    this.tooltipDisabled = false;
  }

  selectBlock(blockId: string) {
    this.configService.getBlockInstance(blockId).subscribe((block) => {
      this.recentlyUsedBlockIds = [blockId, ...this.recentlyUsedBlockIds.filter((id) => id !== blockId).slice(0, 5)];

      this.volatileSettingsService.recentlyUsedBlockIds = this.recentlyUsedBlockIds;

      this.tooltipDisabled = true;
      this.blockSelected.emit(block);
    });
  }

  setcurrentCategory(node: CategoryTreeNode) {
    this.currentCategory = node;
    this.subCategories = node.subCategories;
    this.descriptors = this.searchFilter === '' ? node.descriptors : this.getFilteredDescriptors(node);
  }

  getFilteredDescriptors(node: CategoryTreeNode): BlockDescriptorDto[] {
    return [
      ...node.descriptors.filter((d) => d.name.toLowerCase().includes(this.searchFilter.toLowerCase())),
      ...node.subCategories.flatMap((c) => this.getFilteredDescriptors(c)),
    ];
  }

  getCategories(): CategoryTreeNode[] {
    if (this.currentCategory === null) {
      return [];
    }

    return this.currentCategory.subCategories.filter((c) =>
      c.name.toLowerCase().includes(this.searchFilter.toLowerCase()),
    );
  }

  getDescriptors(): BlockDescriptorDto[] {
    if (this.currentCategory === null) {
      return [];
    }

    return this.currentCategory.descriptors.filter((d) =>
      d.name.toLowerCase().includes(this.searchFilter.toLowerCase()),
    );
  }

  previousCategory() {
    if (this.currentCategory !== null && this.currentCategory.parent !== null) {
      this.setcurrentCategory(this.currentCategory.parent);
    }
  }

  selectCategory(category: CategoryTreeNode) {
    this.setcurrentCategory(category);
  }

  applySearchFilter() {
    // If the filter is empty, simply re-select the current category
    if (this.searchFilter === '') {
      this.setcurrentCategory(this.currentCategory!);
      return;
    }

    if (this.currentCategory !== null) {
      this.subCategories = [];
      this.descriptors = this.getFilteredDescriptors(this.currentCategory);
    }
  }
}
