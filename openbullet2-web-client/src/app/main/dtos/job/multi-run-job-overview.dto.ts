import { JobOverviewDto } from './job.dto';

export interface MultiRunJobOverviewDto extends JobOverviewDto {
  configName: string;
  dataPoolInfo: string;
  bots: number;
  proxyMode: string;
  dataHits: number;
  dataCustom: number;
  dataToCheck: number;
  dataTotal: number;
  dataTested: number;
  cpm: number;
  progress: number;
}
