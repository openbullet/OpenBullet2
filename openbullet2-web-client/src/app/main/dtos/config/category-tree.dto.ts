import { BlockCategoryDto, BlockDescriptorDto, BlockDescriptors } from './block-descriptor.dto';

export class CategoryTreeNode {
  parent: CategoryTreeNode | null = null;
  name: string;
  category: BlockCategoryDto;
  subCategories: CategoryTreeNode[];
  descriptors: BlockDescriptorDto[];

  constructor(dto: CategoryTreeNodeDto, parent: CategoryTreeNode | null, descriptors: BlockDescriptors) {
    this.name = dto.name;
    this.category = dto.category;
    this.parent = parent;
    this.subCategories = dto.subCategories.map((sub) => new CategoryTreeNode(sub, this, descriptors));

    this.descriptors = dto.descriptorIds
      .map((id) => descriptors[id])
      .filter((descriptor): descriptor is BlockDescriptorDto => descriptor !== undefined);
  }

  isRoot(): boolean {
    return this.parent === null;
  }
}

export interface CategoryTreeNodeDto {
  name: string;
  category: BlockCategoryDto;
  subCategories: CategoryTreeNodeDto[];
  descriptorIds: string[];
}
