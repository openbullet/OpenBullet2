import { BlockInstanceTypes } from './block-instance.dto';

export interface ConvertedCSharpDto {
  cSharpScript: string;
}

export interface ConvertedLoliCodeDto {
  loliCode: string;
}

export interface ConvertedStackDto {
  stack: BlockInstanceTypes[];
}
