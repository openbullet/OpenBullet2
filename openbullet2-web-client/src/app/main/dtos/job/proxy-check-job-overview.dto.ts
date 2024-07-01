import { JobOverviewDto } from './job.dto';

export interface ProxyCheckJobOverviewDto extends JobOverviewDto {
  bots: number;
  total: number;
  tested: number;
  working: number;
  notWorking: number;
  cpm: number;
  progress: number;
}
