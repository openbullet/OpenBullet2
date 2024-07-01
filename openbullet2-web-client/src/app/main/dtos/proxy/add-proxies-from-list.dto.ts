import { ProxyType } from '../../enums/proxy-type';

export interface AddProxiesFromListDto {
  defaultType: ProxyType;
  defaultUsername: string;
  defaultPassword: string;
  proxyGroupId: number;
  proxies: string[];
}
