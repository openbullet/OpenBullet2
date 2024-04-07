import { OwnerDto } from '../common/owner.dto';

export interface ProxyGroupDto {
  id: number;
  name: string;
  owner: OwnerDto;
}
