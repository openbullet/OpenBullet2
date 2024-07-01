import { PerformanceInfoDto } from '../dtos/info/performance-info.dto';
import { MRJNewResultMessage } from '../dtos/job/messages/multi-run/new-result.dto';
import { PCJNewResultMessage } from '../dtos/job/messages/proxy-check/new-result.dto';
import { ProxyType } from '../enums/proxy-type';
import { ProxyWorkingStatus } from '../enums/proxy-working-status';

export function getMockedSysPerfMetrics(): PerformanceInfoDto {
  return {
    memoryUsage: Math.floor(Math.random() * 20),
    cpuUsage: Math.random() * 100,
    networkDownload: Math.floor(Math.random() * 20),
    networkUpload: Math.floor(Math.random() * 20),
  };
}

export function getMockedProxyCheckJobNewResultMessage(): PCJNewResultMessage {
  const countries = ['United States', 'Germany', 'France'];
  const octets = [
    Math.floor(Math.random() * 255),
    Math.floor(Math.random() * 255),
    Math.floor(Math.random() * 255),
    Math.floor(Math.random() * 255),
  ];
  return {
    proxyHost: octets.join('.'),
    proxyPort: Math.floor(Math.random() * 65535),
    workingStatus: Math.floor(Math.random() * 2) === 1 ? ProxyWorkingStatus.Working : ProxyWorkingStatus.NotWorking,
    ping: Math.floor(Math.random() * 1000),
    country: countries[Math.floor(Math.random() * countries.length)],
  };
}

export function getMockedMultiRunJobNewResultMessage(): MRJNewResultMessage {
  return {
    dataLine: 'test',
    proxy: {
      type: ProxyType.Http,
      host: '1.1.1.1',
      port: 8080,
      username: null,
      password: null,
    },
    status: 'FAIL',
  };
}
