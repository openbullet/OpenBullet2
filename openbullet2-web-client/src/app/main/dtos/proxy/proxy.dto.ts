import { ProxyType } from '../../enums/proxy-type';
import { ProxyQuality } from '../../enums/proxy-quality';
import { ProxyWorkingStatus } from '../../enums/proxy-working-status';

export interface ProxyDto {
  id: number;
  host: string;
  port: number;
  type: ProxyType;
  username: string;
  password: string;
  country: string;
  status: ProxyWorkingStatus;
  ping: number;
  quality: ProxyQuality;
  lastChecked: string;
  groupId: number;
  groupName: string;
}
