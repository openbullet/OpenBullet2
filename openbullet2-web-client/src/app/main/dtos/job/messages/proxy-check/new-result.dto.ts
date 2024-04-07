import { ProxyWorkingStatus } from 'src/app/main/enums/proxy-working-status';

export interface PCJNewResultMessage {
  proxyHost: string;
  proxyPort: number;
  workingStatus: ProxyWorkingStatus;
  ping: number;
  country: string | null;
}
