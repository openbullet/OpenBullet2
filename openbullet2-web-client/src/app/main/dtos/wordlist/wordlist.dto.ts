import { OwnerDto } from '../common/owner.dto';

export interface WordlistDto {
  id: number;
  name: string;
  filePath: string;
  purpose: string;
  lineCount: number;
  wordlistType: string;
  owner: OwnerDto;
}
