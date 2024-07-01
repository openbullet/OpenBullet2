import { BlockCategoryDto, BlockDescriptorDto, BlockDescriptors } from './block-descriptor.dto';

export class CategoryTreeNode {
  parent: CategoryTreeNode | null = null;
  name: string;
  subCategories: CategoryTreeNode[];
  descriptors: BlockDescriptorDto[];

  constructor(dto: CategoryTreeNodeDto, parent: CategoryTreeNode | null, descriptors: BlockDescriptors) {
    this.name = dto.name;
    this.parent = parent;
    this.subCategories = dto.subCategories.map((sub) => new CategoryTreeNode(sub, this, descriptors));

    this.descriptors = dto.descriptorIds.map((id) => descriptors[id]);
  }

  isRoot(): boolean {
    return this.parent === null;
  }

  getCategory(): BlockCategoryDto | null {
    // If the category is RuriLib or RuriLib > Blocks,
    // return a custom category
    if (this.name === 'RuriLib') {
      return {
        name: 'RuriLib',
        description: 'Blocks from the RuriLib standard library',
        backgroundColor: '#af4304',
        foregroundColor: 'white',
      };
    }

    if (this.name === 'Blocks' && this.parent?.name === 'RuriLib') {
      return {
        name: 'Blocks',
        description: 'Blocks from the RuriLib standard library',
        backgroundColor: '#af4304',
        foregroundColor: 'white',
      };
    }

    if (this.descriptors.length > 0) {
      return this.descriptors[0].category;
    }

    if (this.subCategories.length > 0) {
      return this.subCategories[0].getCategory();
    }

    return null;
  }
}

export interface CategoryTreeNodeDto {
  name: string;
  subCategories: CategoryTreeNodeDto[];
  descriptorIds: string[];
}
