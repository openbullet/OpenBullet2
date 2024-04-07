import { ProxyType } from '../../enums/proxy-type';
import { ProxyWorkingStatus } from '../../enums/proxy-working-status';

export enum ProxySortField {
  Host = 'host',
  Port = 'port',
  Username = 'username',
  Password = 'password',
  Country = 'country',
  Ping = 'ping',
  LastChecked = 'lastChecked',
}

export interface ProxyFiltersDto {
  pageNumber: number | null;
  pageSize: number | null;
  proxyGroupId: number;
  searchTerm: string | null;
  type: ProxyType | null;
  status: ProxyWorkingStatus | null;
  sortBy: ProxySortField | null;
  sortDescending: boolean;
}
