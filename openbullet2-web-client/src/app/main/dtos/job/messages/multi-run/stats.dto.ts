import { MRJDataStatsDto, MRJProxyStatsDto } from '../../multi-run-job.dto';

export interface MRJStatsMessage {
  dataStats: MRJDataStatsDto;
  proxyStats: MRJProxyStatsDto;
  cpm: number;
  captchaCredit: number;
  elapsed: string;
  remaining: string;
  progress: number;
}
