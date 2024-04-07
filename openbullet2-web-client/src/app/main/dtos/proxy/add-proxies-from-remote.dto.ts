import { ProxyType } from '../../enums/proxy-type';

export interface AddProxiesFromRemoteDto {
  defaultType: ProxyType;
  defaultUsername: string;
  defaultPassword: string;
  proxyGroupId: number;
  url: string;
}
