import { ProxyType } from '../../enums/proxy-type';
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
  lastChecked: string;
  groupId: number;
  groupName: string;
}
