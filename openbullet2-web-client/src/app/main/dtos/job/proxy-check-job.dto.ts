import { JobDto } from './job.dto';
import { AbsoluteTimeStartConditionDto, RelativeTimeStartConditionDto } from './start-condition.dto';

export interface ProxyCheckJobDto extends JobDto {
  bots: number;
  startCondition: RelativeTimeStartConditionDto | AbsoluteTimeStartConditionDto;
  groupId: number;
  groupName: string;
  checkOnlyUntested: boolean;
  target: ProxyCheckTargetDto | null;
  timeoutMilliseconds: number;
  // biome-ignore lint/suspicious/noExplicitAny: Polymorphic
  checkOutput: any;
  total: number;
  tested: number;
  working: number;
  notWorking: number;
  cpm: number;
  elapsed: string;
  remaining: string;
  progress: number;
}

export interface ProxyCheckTargetDto {
  url: string;
  successKey: string;
}
