import { JobOverviewDto } from './job.dto';

export interface MultiRunJobOverviewDto extends JobOverviewDto {
  configName: string;
  dataPoolInfo: string;
  bots: number;
  useProxies: boolean;
  dataHits: number;
  dataCustom: number;
  dataToCheck: number;
  dataTotal: number;
  dataTested: number;
  cpm: number;
  progress: number;
}
