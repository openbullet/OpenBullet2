import { OwnerDto } from '../common/owner.dto';

export interface HitDto {
  id: number;
  data: string;
  capturedData: string;
  proxy: string;
  date: string;
  type: string;
  owner: OwnerDto;
  configId: string | null;
  configName: string | null;
  configCategory: string | null;
  wordlistId: number; // -1 if no wordlist
  wordlistName: string | null;
}
